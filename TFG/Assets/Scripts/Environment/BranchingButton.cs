using UnityEngine;

public class BranchingButton : MonoBehaviour, IInteractable
{
    public bool isTrueButton;

    public void HandleInteraction(PlayerController player) => PressButton();

    public void PressButton()
    {
        GameManager.Instance.ValidateAction(GameManager.TaskState.PRESS_JMP, "JUMP", isTrueButton ? "TRUE" : "FALSE");
    }
}
