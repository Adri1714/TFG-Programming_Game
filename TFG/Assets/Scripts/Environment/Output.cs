using UnityEngine;

public class Output : MonoBehaviour, IInteractable
{
    public void HandleInteraction(PlayerController player)
    {
        GameManager gm = GameManager.Instance;
        if (player.carriedCube == null) return;

        DataPacket packet = player.carriedCube.GetComponent<DataPacket>();

        if (gm.CurrentTaskNeedsAlu && !packet.fromAlu)
        {
            AudioManager.Play(l => l.error);
            return;
        }
        if (gm.CurrentTaskNeedsWorkMem && !packet.fromWorkMem)
        {
            AudioManager.Play(l => l.error);
            return;
        }

        if (gm.ValidateAction(GameManager.TaskState.WRITE_IO, "STDOUT", packet.value))
            player.ConsumeCube();
    }
}
