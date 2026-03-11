using UnityEngine;
using TMPro;

// ALU simple: recull dos operands, calcula i retorna el resultat en un cub. (DE MOMENT NMES SUMA)
public class ALUController : MonoBehaviour
{
    public Transform leftSlot, rightSlot, resultSlot;
    public GameObject cubePrefab;
    public string currentOp = "+";

    [Header("Pantalles Visuals (UI)")]
    public TMP_Text leftLabel;
    public TMP_Text rightLabel;

    private string leftVal = "", rightVal = "";

    void Start()
    {
        UpdateLabels();
    }

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

    // Executa l'operacio actual i deixa la maquina preparada per al seguent calcul.
    public void Calculate()
    {
        int a = int.Parse(leftVal);
        int b = int.Parse(rightVal);
        int res = (currentOp == "+") ? (a + b) : 0; 

        GameObject resCube = Instantiate(cubePrefab, resultSlot.position, Quaternion.identity);
        resCube.GetComponent<DataPacket>().SetData(res.ToString(), false);

        leftVal = ""; rightVal = "";
        UpdateLabels(); 
        
        Debug.Log($"ALU: {a} {currentOp} {b} = {res}");
    }

    // Refresca els displays de cada operand.
    private void UpdateLabels()
    {
        if (leftLabel != null) 
            leftLabel.text = string.IsNullOrEmpty(leftVal) ? "[ ]" : leftVal;
            
        if (rightLabel != null) 
            rightLabel.text = string.IsNullOrEmpty(rightVal) ? "[ ]" : rightVal;
    }
}