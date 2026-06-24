using UnityEngine;

public class PanelActivator : MonoBehaviour, IInteractable
{
    public void HandleInteraction(PlayerController player)
    {
        if (player.carriedCube == null)
        {
            ALUController aluController = FindFirstObjectByType<ALUController>();
            if (aluController != null)
            {
                aluController.OpenOperatorPanel();
            }
        }
    }
}
