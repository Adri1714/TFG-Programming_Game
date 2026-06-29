using UnityEngine;

public class WorkMemorySlot : MemorySlot
{
    public GameObject cubePrefab;
    public Transform spawnPoint;

    public override void HandleInteraction(PlayerController player)
    {
        GameManager gm = GameManager.Instance;

        if (player.carriedCube != null)
        {
            DataPacket packet = player.carriedCube.GetComponent<DataPacket>();

            if (gm.currentTaskState == GameManager.TaskState.DIM_MEM && packet.isIdentifier)
            {
                if (varName != "???")
                {
                    Debug.LogWarning($"[RAM] Aquest calaix ja conte '{varName}'. Usa'n un de buit.");
                    return;
                }

                if (gm.ValidateAction(GameManager.TaskState.DIM_MEM, "RAM_SLOT", packet.value))
                {
                    varName = packet.value;
                    displayName = varName;
                    RefreshLabel();
                    player.ConsumeCube();
                }
            }
            else if (gm.currentTaskState == GameManager.TaskState.WRITE_MEM && !packet.isIdentifier)
            {
                string previousValue = value;
                string previousDisplay = displayName;

                value = packet.value;
                displayName = $"{varName} = {value}";
                RefreshLabel();
                if (gm.CurrentTaskNeedsAlu && !packet.fromAlu)
                {
                    value = previousValue;
                    displayName = previousDisplay;
                    RefreshLabel();
                    AudioManager.Play(l => l.error);
                    return;
                }
                
                if (gm.CurrentTaskNeedsWorkMem && !packet.fromWorkMem)
                {
                    value = previousValue;
                    displayName = previousDisplay;
                    RefreshLabel();
                    AudioManager.Play(l => l.error);
                    return;
                }

                if (gm.ValidateAction(GameManager.TaskState.WRITE_MEM, this.varName, packet.value))
                {
                    player.ConsumeCube();
                }
                else
                {
                    value = previousValue;
                    displayName = previousDisplay;
                    RefreshLabel();
                }
            }
        }
        else if (varName != "???" && value != "???")
        {
            SpawnValueCube();
        }
    }

    private void SpawnValueCube()
    {
        GameObject newCube = Instantiate(cubePrefab, spawnPoint.position + new Vector3(0, -1f, 0), Quaternion.identity);
        newCube.GetComponent<DataPacket>().SetData(value, false, false, true);
    }
}
