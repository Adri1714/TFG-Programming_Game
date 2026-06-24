using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ALUController : MonoBehaviour, IInteractable
{
    public Transform leftSlot, rightSlot, resultSlot;
    public GameObject cubePrefab;

    [Header("Pantalles Visuals (UI)")]
    public TMP_Text leftLabel;
    public TMP_Text rightLabel;
    public TMP_Text expressionLabel;             
    public GameObject calculatorPanel;
    public GameObject iluminatedCalculatorPanel;

    [Header("Panell d'operador")]
    [SerializeField] private GameObject operatorPanel;
    [SerializeField] private float resultDisplayTime = 1.2f;
    [SerializeField] private Button executeButton;

    [Header("Rodeta d'operador")]
    [SerializeField] private RectTransform operatorDial;
    [SerializeField] private float anglePerStep = 90f;
    [SerializeField] private float startAngle = 0f;
    [SerializeField] private float rotateSpeed = 12f;

    private readonly string[] operators = { "+", "-", "*", "/" };

    private enum AluState { Idle, ReadyToOpen, AwaitingOperator, ShowingResult }
    private AluState state = AluState.Idle;

    private string leftVal = "", rightVal = "", selectedOp = "";
    private int opIndex;
    private float targetAngle;

    void Start()
    {
        UpdateLabels();
        if (operatorPanel != null) operatorPanel.SetActive(false);
        if (iluminatedCalculatorPanel != null) iluminatedCalculatorPanel.SetActive(false);
        if (calculatorPanel != null) calculatorPanel.SetActive(true);
    }

    void Update()
    {
        if (state != AluState.AwaitingOperator || operatorDial == null) return;

        float current = operatorDial.localEulerAngles.z;
        if (Mathf.Abs(Mathf.DeltaAngle(current, targetAngle)) < 0.05f) return;

        float z = Mathf.LerpAngle(current, targetAngle, Time.unscaledDeltaTime * rotateSpeed);
        operatorDial.localEulerAngles = new Vector3(0f, 0f, z);
    }

    public void HandleInteraction(PlayerController player)
    {
        GameManager gm = GameManager.Instance;
        if (gm != null && !gm.CurrentTaskNeedsAlu) return;

        if (state == AluState.ReadyToOpen)
        {
            OpenOperatorPanel();
            return;
        }

        if (state != AluState.Idle || player.carriedCube == null) return;

        DataPacket packet = player.carriedCube.GetComponent<DataPacket>();
        if (packet == null) return;

        AudioManager.Play(l => l.declareVar);

        if (string.IsNullOrEmpty(leftVal))
        {
            leftVal = packet.value;
            player.ConsumeCube();
            UpdateLabels();
        }
        else if (string.IsNullOrEmpty(rightVal))
        {
            rightVal = packet.value;
            player.ConsumeCube();
            UpdateLabels();
            state = AluState.ReadyToOpen;
            if (calculatorPanel != null) calculatorPanel.SetActive(false);
            if (iluminatedCalculatorPanel != null) iluminatedCalculatorPanel.SetActive(true);
        }
    }

    // Obre el panell amb la primera operació ja seleccionada.
    public void OpenOperatorPanel()
    {
        state = AluState.AwaitingOperator;
        opIndex = 0;
        selectedOp = operators[0];
        targetAngle = startAngle;
        if (operatorDial != null) operatorDial.localEulerAngles = new Vector3(0f, 0f, startAngle);
        if (operatorPanel != null) operatorPanel.SetActive(true);
        RefreshExpression();
    }

    public void CycleOperator()
    {
        if (state != AluState.AwaitingOperator) return;
        opIndex = (opIndex + 1) % operators.Length;
        selectedOp = operators[opIndex];
        targetAngle = startAngle - opIndex * anglePerStep;
        AudioManager.Play(l => l.buttonClick);
        RefreshExpression();
    }

    public void ExecuteOperation()
    {
        if (state != AluState.AwaitingOperator || string.IsNullOrEmpty(selectedOp)) return;

        if (!int.TryParse(leftVal, out int a) || !int.TryParse(rightVal, out int b))
        {
            state = AluState.ShowingResult;
            StartCoroutine(CloseAfterDelay());
            return;
        }

        if (selectedOp == "/" && b == 0)
        {
            Debug.LogWarning("ALU: divisió per zero.");
            return;
        }

        int res = selectedOp switch
        {
            "+" => a + b,
            "-" => a - b,
            "*" => a * b,
            "/" => a / b,
            _ => 0
        };
        GameManager gm = GameManager.Instance;
        if (gm != null && !gm.MatchesExpectedOperation(a, b, selectedOp))
        {
            AudioManager.Play(l => l.error);
            state = AluState.ShowingResult;
            StartCoroutine(CloseAfterDelay());
            return;
        }

        if (expressionLabel != null)
            expressionLabel.text = $"{leftVal} {OpSymbol(selectedOp)} {rightVal}";

        AudioManager.Play(l => l.aluOperate);
        GameObject resCube = Instantiate(cubePrefab, resultSlot.position, Quaternion.identity);
        resCube.GetComponent<DataPacket>().SetData(res.ToString(), false, true);

        state = AluState.ShowingResult;
        StartCoroutine(CloseAfterDelay());
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(resultDisplayTime);
        ResetAlu();
    }

    private void ResetAlu()
    {
        leftVal = ""; rightVal = ""; selectedOp = "";
        opIndex = 0;
        targetAngle = startAngle;
        state = AluState.Idle;
        if (iluminatedCalculatorPanel != null) iluminatedCalculatorPanel.SetActive(false);
        if (calculatorPanel != null) calculatorPanel.SetActive(true);
        if (operatorPanel != null) operatorPanel.SetActive(false);
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        if (leftLabel != null)  leftLabel.text  = string.IsNullOrEmpty(leftVal)  ? "_" : leftVal;
        if (rightLabel != null) rightLabel.text = string.IsNullOrEmpty(rightVal) ? "_" : rightVal;
        RefreshExpression();
    }

    private void RefreshExpression()
    {
        if (expressionLabel == null) return;
        string l = string.IsNullOrEmpty(leftVal)  ? "_" : leftVal;
        string r = string.IsNullOrEmpty(rightVal) ? "_" : rightVal;
        expressionLabel.text = $"{l} {OpSymbol(selectedOp)} {r}";
    }

    private string OpSymbol(string op) => op switch
    {
        "+" => "+",
        "-" => "−",
        "*" => "×",
        "/" => "÷",
        _   => "?"
    };
}