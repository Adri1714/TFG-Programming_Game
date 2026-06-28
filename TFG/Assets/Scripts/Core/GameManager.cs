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
    private Calculator calculator;

    [Header("Estat del Simulador")]
    public IReadOnlyDictionary<string, Fragment> Fragments => _fragments;
    private readonly Dictionary<string, Fragment> _fragments = new();
    private List<Fragment> sortedFragments = new List<Fragment>();
    private int currentFragmentIndex = 0;

    // Font de veritat dels valors de variables, independent dels slots fisics.
    private readonly Dictionary<string, string> variables = new();

    public enum TaskState { NONE, DIM_MEM, WRITE_MEM, WRITE_IO, PRESS_JMP }
    public TaskState currentTaskState = TaskState.NONE;
    // True quan el valor de la tasca actual ve d'una operació (cal usar la ALU).
    public bool CurrentTaskNeedsAlu { get; private set; }
    private string expectedValue;
    private string expectedDestination;
    private int expOpA, expOpB;
    private char expOpChar;
    private bool taskHasOperation;

    public event Action<List<string>> OnCodeLoaded;
    public event Action<int> OnLineChanged;
    public event Action<string> OnTaskUpdated;
    public event Action<string> OnOutputGenerated;
    public event Action OnProgramFinished;
    public event Action<TaskState> OnTaskStateChanged;
    public event Action<TaskState> OnActionSucceeded;
    public event Action OnActionFailed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        parser = GetComponent<LevelParser>();
        calculator = new Calculator(GetVariableValue, workMemory.GetVariableValue);

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
            Debug.LogWarning($"[GameManager] '{path}' not found in Resources. Using levelFile from the Inspector.");

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
    private void SetExpectedOperation(string expr)
    {
        taskHasOperation = false;
        string clean = expr.Replace("(", "").Replace(")", "").Replace('×','*').Replace('÷','/').Trim();
        foreach (char op in new[] { '+', '-', '*', '/' })
        {
            int idx = clean.IndexOf(op);
            if (idx <= 0) continue;
            if (int.TryParse(calculator.Evaluate(clean.Substring(0, idx)), out expOpA) &&
                int.TryParse(calculator.Evaluate(clean.Substring(idx + 1)), out expOpB))
            {
                expOpChar = op;
                taskHasOperation = true;
            }
            return;
        }
    }
    public bool MatchesExpectedOperation(int a, int b, string op)
    {
        if (!taskHasOperation) return true;
        char o = op.Length > 0 ? op[0] : ' ';
        if (o != expOpChar) return false;

        if (o == '+' || o == '*')
            return (a == expOpA && b == expOpB) || (a == expOpB && b == expOpA);
        return a == expOpA && b == expOpB;
    }

    public bool HasFragment(string id) => _fragments.ContainsKey(id);
    public void RegisterFragment(string id, Fragment f) => _fragments.TryAdd(id, f);
    public void AddCommandToFragment(string id, string cmd) => _fragments[id].AddCommand(cmd);

    // Situa el PC al fragment indicat i notifica la linia activa a la UI.
    public void SetFragment(int index)
    {
        if (index >= sortedFragments.Count) {
            OnTaskUpdated?.Invoke("PROGRAM FINISHED!");
            OnTaskStateChanged?.Invoke(TaskState.NONE);
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
        if (command == null) { ExecuteNext(); return; }
        string[] parts = command.Split(':');
        CurrentTaskNeedsAlu = false;

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
                SetTask(TaskState.DIM_MEM, "RAM_SLOT", parts[1], $"Declare variable: {parts[1]}");
                break;
            case "WRITE_MEM":
                string valMem = calculator.Evaluate(parts[2]);
                CurrentTaskNeedsAlu = HasOperator(parts[2]);
                SetExpectedOperation(parts[2]); 
                SetTask(TaskState.WRITE_MEM, parts[1], valMem, $"Assign value {valMem} to {parts[1]}");
                break;
            case "WRITE_IO":
                string valIO = calculator.Evaluate(parts[1]);
                CurrentTaskNeedsAlu = HasOperator(parts[1]);
                SetExpectedOperation(parts[1]);
                SetTask(TaskState.WRITE_IO, "STDOUT", valIO, $"Print: {valIO}");
                break;
            case "PRESS_JMP":
                SetTask(TaskState.PRESS_JMP, "JUMP", parts[1], $"Evaluate: {parts[1]}", parts[2]);
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
        OnTaskStateChanged?.Invoke(state);
    }

    // L'expressió conté un operador? (llavors el valor s'ha de calcular a la ALU)
    private static bool HasOperator(string expr) =>
        expr.IndexOfAny(new[] { '+', '-', '*', '/', '×', '÷' }) >= 0;

    // Comprova si l'accio del jugador coincideix amb la tasca pendent i avanca.
    public bool ValidateAction(TaskState action, string destination, string value)
    {
        if (currentTaskState != action) return false;

        bool success = false;

        if (action == TaskState.PRESS_JMP) {
            string realResult = calculator.Evaluate(expectedValue).ToUpper();
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
            OnActionSucceeded?.Invoke(action);
            if (action == TaskState.WRITE_MEM) variables[expectedDestination] = expectedValue;
            if (action == TaskState.WRITE_IO) OnOutputGenerated?.Invoke(value);
            currentTaskState = TaskState.NONE;
            OnTaskStateChanged?.Invoke(TaskState.NONE);
            ExecuteNext();
            return true;
        }
        else {
            OnActionFailed?.Invoke();
            return false;
        }
    }

    private void JumpToID(string id) {
        int idx = sortedFragments.FindIndex(f => f.identifier == id);
        if (idx != -1) SetFragment(idx);
    }    

    private string GetVariableValue(string name)
    {
        return variables.TryGetValue(name, out string value) ? value : null;
    }
}
