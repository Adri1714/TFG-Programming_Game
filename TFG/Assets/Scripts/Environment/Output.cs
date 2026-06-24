using UnityEngine;

public class Output : MonoBehaviour, IInteractable
{
    public void HandleInteraction(PlayerController player)
    {
        if (player.carriedCube == null) return;

        DataPacket packet = player.carriedCube.GetComponent<DataPacket>();

        if (GameManager.Instance.CurrentTaskNeedsAlu && !packet.fromAlu)
        {
            AudioManager.Play(l => l.error);
            return;
        }

        if (GameManager.Instance.ValidateAction(GameManager.TaskState.WRITE_IO, "STDOUT", packet.value))
            player.ConsumeCube();
    }
}
