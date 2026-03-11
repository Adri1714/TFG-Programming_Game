using UnityEngine;

// Botó de decisio booleana per a l'ordre condicional de salt.
public class BranchingButton : MonoBehaviour
{
    public bool isTrueButton;
    public Material activeMat;
    public Material idleMat;

    // Notifica al GameManager quina branca ha triat el jugador.
    public void PressButton()
    {
        GameManager.Instance.ValidateAction(GameManager.TaskState.PRESS_JMP, "JUMP", isTrueButton ? "TRUE" : "FALSE");
    }
}