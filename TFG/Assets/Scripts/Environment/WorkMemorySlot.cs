using UnityEngine;

public class WorkMemorySlot : MemorySlot
{
    public GameObject cubePrefab;
    public Transform spawnPoint;

    // Declara/escriu segons la tasca i si el jugador porta cub; si no, llegeix.
    public override void HandleInteraction(PlayerController player)
    {
        GameManager gm = GameManager.Instance;

        if (player.carriedCube != null)
        {
            DataPacket packet = player.carriedCube.GetComponent<DataPacket>();

            if (gm.currentTaskState == GameManager.TaskState.DIM_MEM && packet.isIdentifier)
            {
                // Nomes es pot declarar en un calaix buit (no sobreescriure variables).
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
                // Comprometem el valor abans de validar perque ValidateAction encadena ExecuteNext().
                string previousValue = value;
                string previousDisplay = displayName;

                value = packet.value;
                displayName = $"{varName} = {value}";
                RefreshLabel();

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
        GameObject newCube = Instantiate(cubePrefab, spawnPoint.position, Quaternion.identity);
        newCube.GetComponent<DataPacket>().SetData(value, false);
    }
}
