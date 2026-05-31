using UnityEngine;

public class Output : MonoBehaviour, IInteractable
{
    // Consumeix el cub nomes quan l'accio esperada es correcta.
    public void HandleInteraction(PlayerController player)
    {
        if (player.carriedCube == null) return;

        DataPacket packet = player.carriedCube.GetComponent<DataPacket>();

        if (GameManager.Instance.ValidateAction(GameManager.TaskState.WRITE_IO, "STDOUT", packet.value))
            player.ConsumeCube();
    }
}
