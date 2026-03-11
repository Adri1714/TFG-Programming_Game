using UnityEngine;
using System;
using System.Collections.Generic;

// Orquestra el flux d'execucio del programa i valida les accions del jugador.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuració")]
    public TextAsset levelFile;
    public LevelParser parser;
    public CodeMemoryManager codeMemory;
    public WorkMemoryManager workMemory;

    [Header("Estat del Simulador")]
    public Dictionary<string, Fragment> fragments = new Dictionary<string, Fragment>();
    private List<Fragment> sortedFragments = new List<Fragment>();
    private int currentFragmentIndex = 0;

    public enum TaskState { NONE, DIM_MEM, WRITE_MEM, WRITE_IO, PRESS_JMP }
    public TaskState currentTaskState = TaskState.NONE;
    private string expectedValue;
    private string expectedDestination;

    public event Action<List<string>> OnCodeLoaded;
    public event Action<int> OnLineChanged;
    public event Action<string> OnTaskUpdated;
    public event Action<string> OnOutputGenerated;

    private void Awake() => Instance = this;

    private void Start()
    {
        parser = GetComponent<LevelParser>();
        parser.ParseLevel(levelFile, this);
    }

    // Ordena fragments per ID numeric i inicia l'execucio pel primer.
    public void InitializeExecution()
    {
        sortedFragments = new List<Fragment>(fragments.Values);
        sortedFragments.Sort((a, b) => int.Parse(a.identifier).CompareTo(int.Parse(b.identifier)));
        
        OnCodeLoaded?.Invoke(new List<string>(parser.GetSourceCodeLines()));
        SetFragment(0);
    }

    // Situa el PC al fragment indicat i notifica la linia activa a la UI.
    public void SetFragment(int index)
    {
        if (index >= sortedFragments.Count) {
            OnTaskUpdated?.Invoke("PROGRAMA FINALITZAT!");
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

    // Guarda l'accio esperada i publica el prompt a la UI.
    private void SetTask(TaskState state, string dest, string val, string prompt, string extra = "")
    {
        currentTaskState = state;
        expectedDestination = dest;
        expectedValue = val;
        if (state == TaskState.PRESS_JMP) expectedDestination = extra;
        OnTaskUpdated?.Invoke(prompt);
    }

    // Comprova si l'accio del jugador coincideix amb la tasca pendent.
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
            Debug.Log("TASCA COMPLETADA!");
            if (action == TaskState.WRITE_IO) OnOutputGenerated?.Invoke(value);
            currentTaskState = TaskState.NONE;
            ExecuteNext();
            return true;
        } else {
            Debug.Log($"ERROR: S'esperava {expectedValue} a {expectedDestination}, però s'ha rebut {value} a {destination}");
            return false;
        }
    }

    // Salt directe al fragment amb ID concret (JMP).
    private void JumpToID(string id) {
        int idx = sortedFragments.FindIndex(f => f.identifier == id);
        if (idx != -1) SetFragment(idx);
    }

    // Avalua expressions simples (comparacio, suma, literals i variables RAM).
    public string Evaluate(string expr) 
    {
        string cleanExpr = expr.Replace("(", "").Replace(")", "").Replace("\"", "").Trim();
        if (cleanExpr.Contains("<")) {
            string[] p = cleanExpr.Split('<');
            return (int.Parse(Evaluate(p[0])) < int.Parse(Evaluate(p[1]))) ? "TRUE" : "FALSE";
        }
        if (cleanExpr.Contains("+")) {
            string[] p = cleanExpr.Split('+');
            return (int.Parse(Evaluate(p[0])) + int.Parse(Evaluate(p[1]))).ToString();
        }
        if (int.TryParse(cleanExpr, out _)) return cleanExpr;
        
        string ramValue = workMemory.GetVariableValue(cleanExpr);
        if (ramValue != null) return ramValue;
        
        return cleanExpr;
    }
}