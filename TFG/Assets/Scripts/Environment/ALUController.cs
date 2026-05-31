using UnityEngine;
using TMPro;

public class ALUController : MonoBehaviour, IInteractable
{
    public Transform leftSlot, rightSlot, resultSlot;
    public GameObject cubePrefab;
    public string currentOp = "+";

    [Header("Pantalles Visuals (UI)")]
    public TMP_Text leftLabel;
    public TMP_Text rightLabel;

    private string leftVal = "", rightVal = "";

    void Start() => UpdateLabels();

    // Accepta cubs en ordre esquerra-dreta i calcula en completar la parella.
    public void HandleInteraction(PlayerController player)
    {
        if (player.carriedCube == null) return;

        DataPacket packet = player.carriedCube.GetComponent<DataPacket>();

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
            Calculate();
        }
    }

    // Executa l'operacio i deixa la maquina preparada per al seguent calcul.
    public void Calculate()
    {
        if (!int.TryParse(leftVal, out int a) || !int.TryParse(rightVal, out int b))
        {
            Debug.LogWarning("ALU: Operands no numèrics, no es pot calcular.");
            leftVal = ""; rightVal = "";
            UpdateLabels();
            return;
        }

        int res = (currentOp == "+") ? (a + b) : 0;

        GameObject resCube = Instantiate(cubePrefab, resultSlot.position, Quaternion.identity);
        resCube.GetComponent<DataPacket>().SetData(res.ToString(), false);

        leftVal = ""; rightVal = "";
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        if (leftLabel != null)
            leftLabel.text = string.IsNullOrEmpty(leftVal) ? "[ ]" : leftVal;

        if (rightLabel != null)
            rightLabel.text = string.IsNullOrEmpty(rightVal) ? "[ ]" : rightVal;
    }
}
