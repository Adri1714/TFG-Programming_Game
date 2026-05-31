using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuració")]
    public TextAsset levelFile;
    public LevelParser parser;
    public CodeMemoryManager codeMemory;
    public WorkMemoryManager workMemory;

    [Header("Estat del Simulador")]
    public IReadOnlyDictionary<string, Fragment> Fragments => _fragments;
    private readonly Dictionary<string, Fragment> _fragments = new();
    private List<Fragment> sortedFragments = new List<Fragment>();
    private int currentFragmentIndex = 0;

    // Font de veritat dels valors de variables, independent dels slots fisics.
    private readonly Dictionary<string, string> variables = new();

    public enum TaskState { NONE, DIM_MEM, WRITE_MEM, WRITE_IO, PRESS_JMP }
    public TaskState currentTaskState = TaskState.NONE;
    private string expectedValue;
    private string expectedDestination;

    public event Action<List<string>> OnCodeLoaded;
    public event Action<int> OnLineChanged;
    public event Action<string> OnTaskUpdated;
    public event Action<string> OnOutputGenerated;
    public event Action OnProgramFinished;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        parser = GetComponent<LevelParser>();

        TextAsset selectedLevel = LoadSelectedLevel();
        if (selectedLevel != null) levelFile = selectedLevel;

        parser.ParseLevel(levelFile, this);
    }

    // Carrega el nivell triat al menu des de Resources/Levels/.
    private TextAsset LoadSelectedLevel()
    {
        if (string.IsNullOrEmpty(GameSession.SelectedLevel)) return null;

        string path = GameSession.LevelResourcePath + GameSession.SelectedLevel;
        TextAsset asset = Resources.Load<TextAsset>(path);

        if (asset == null)
            Debug.LogWarning($"[GameManager] No s'ha trobat '{path}' a Resources. S'usa el levelFile de l'Inspector.");

        return asset;
    }

    // Ordena fragments per ID numeric i inicia l'execucio pel primer.
    public void InitializeExecution()
    {
        variables.Clear();
        sortedFragments = new List<Fragment>(Fragments.Values);
        sortedFragments.Sort((a, b) => int.Parse(a.identifier).CompareTo(int.Parse(b.identifier)));

        OnCodeLoaded?.Invoke(new List<string>(parser.GetSourceCodeLines()));
        SetFragment(0);
    }

    public bool HasFragment(string id) => _fragments.ContainsKey(id);
    public void RegisterFragment(string id, Fragment f) => _fragments.TryAdd(id, f);
    public void AddCommandToFragment(string id, string cmd) => _fragments[id].AddCommand(cmd);

    // Situa el PC al fragment indicat i notifica la linia activa a la UI.
    public void SetFragment(int index)
    {
        if (index >= sortedFragments.Count) {
            OnTaskUpdated?.Invoke("PROGRAMA FINALITZAT!");
            OnProgramFinished?.Invoke();
            return;
        }

        currentFragmentIndex = index;
        Fragment activeFrag = sortedFragments[currentFragmentIndex];
        activeFrag.BeginIterate();

        OnLineChanged?.Invoke(activeFrag.line);
        ExecuteNext();
    }

    // Interpreta i executa la seguent comanda del fragment actual.
    public void ExecuteNext()
    {
        Fragment currentFrag = sortedFragments[currentFragmentIndex];
        if (currentFrag.IsFinished()) {
            SetFragment(currentFragmentIndex + 1);
            return;
        }

        string command = currentFrag.NextCommand();
        string[] parts = command.Split(':');

        switch (parts[0])
        {
            case "DIM_ID":
                codeMemory.SpawnInstructionData(int.Parse(parts[1]), parts[2], true);
                ExecuteNext();
                break;
            case "DIM_CODE":
                codeMemory.SpawnInstructionData(int.Parse(parts[1]), parts[2].Replace("\"", ""), false);
                ExecuteNext();
                break;
            case "DIM_MEM":
                SetTask(TaskState.DIM_MEM, "RAM_SLOT", parts[1], $"Declara la variable: {parts[1]}");
                break;
            case "WRITE_MEM":
                string valMem = Evaluate(parts[2]);
                SetTask(TaskState.WRITE_MEM, parts[1], valMem, $"Assigna {valMem} a {parts[1]}");
                break;
            case "WRITE_IO":
                string valIO = Evaluate(parts[1]);
                SetTask(TaskState.WRITE_IO, "STDOUT", valIO, $"Imprimeix: {valIO}");
                break;
            case "PRESS_JMP":
                SetTask(TaskState.PRESS_JMP, "JUMP", parts[1], $"Avalua: {parts[1]}", parts[2]);
                break;
            case "JMP":
                JumpToID(parts[1]);
                break;
            default:
                ExecuteNext();
                break;
        }
    }

    private void SetTask(TaskState state, string dest, string val, string prompt, string extra = "")
    {
        currentTaskState = state;
        expectedDestination = dest;
        expectedValue = val;
        if (state == TaskState.PRESS_JMP) expectedDestination = extra;
        OnTaskUpdated?.Invoke(prompt);
    }

    // Comprova si l'accio del jugador coincideix amb la tasca pendent i avanca.
    public bool ValidateAction(TaskState action, string destination, string value)
    {
        if (currentTaskState != action) return false;

        bool success = false;

        if (action == TaskState.PRESS_JMP) {
            string realResult = Evaluate(expectedValue).ToUpper();
            success = (value.ToUpper() == realResult);
            if (success && realResult == "FALSE") {
                JumpToID(expectedDestination);
                return true;
            }
        }
        else {
            success = (destination == expectedDestination && value == expectedValue);
        }

        if (success) {
            if (action == TaskState.WRITE_MEM) variables[expectedDestination] = expectedValue;
            if (action == TaskState.WRITE_IO) OnOutputGenerated?.Invoke(value);
            currentTaskState = TaskState.NONE;
            ExecuteNext();
            return true;
        }

        Debug.Log($"ERROR: S'esperava {expectedValue} a {expectedDestination}, però s'ha rebut {value} a {destination}");
        return false;
    }

    private void JumpToID(string id) {
        int idx = sortedFragments.FindIndex(f => f.identifier == id);
        if (idx != -1) SetFragment(idx);
    }

    // Avalua expressions simples: comparacio, suma, literals i variables.
    public string Evaluate(string expr)
    {
        string cleanExpr = expr.Replace("(", "").Replace(")", "").Replace("\"", "").Trim();
        if (cleanExpr.Contains("<")) {
            string[] p = cleanExpr.Split('<');
            if (TryEvalInt(p[0], out int left) && TryEvalInt(p[1], out int right))
                return (left < right) ? "TRUE" : "FALSE";
            return "FALSE";
        }
        if (cleanExpr.Contains("+")) {
            string[] p = cleanExpr.Split('+');
            if (TryEvalInt(p[0], out int left) && TryEvalInt(p[1], out int right))
                return (left + right).ToString();
            return cleanExpr;
        }
        if (int.TryParse(cleanExpr, out _)) return cleanExpr;

        if (variables.TryGetValue(cleanExpr, out string varValue)) return varValue;

        string ramValue = workMemory.GetVariableValue(cleanExpr);
        if (ramValue != null) return ramValue;

        return cleanExpr;
    }

    // Converteix una sub-expressio a enter, avisant si no es numerica en lloc de petar.
    private bool TryEvalInt(string expr, out int result)
    {
        string evaluated = Evaluate(expr);
        if (int.TryParse(evaluated, out result)) return true;

        Debug.LogError($"[GameManager] No s'ha pogut avaluar '{expr.Trim()}' com a numero (valor obtingut: '{evaluated}').");
        result = 0;
        return false;
    }
}
