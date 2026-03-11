using UnityEngine;
using TMPro;

// Slot de RAM: declaracio de variable, escriptura de valor i lectura.
public class WorkMemorySlot : MemorySlot
{
    public GameObject cubePrefab;
    public Transform spawnPoint;

    // Escriu o llegeix segons l'estat de tasca i si el jugador porta cub.
    public override void HandleInteraction(PlayerController player)
    {
        GameManager gm = GameManager.Instance;

        if (player.carriedCube != null)
        {
            DataPacket packet = player.carriedCube.GetComponent<DataPacket>();
            
            if (gm.currentTaskState == GameManager.TaskState.DIM_MEM && packet.isIdentifier)
            {
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
                if (gm.ValidateAction(GameManager.TaskState.WRITE_MEM, this.varName, packet.value))
                {
                    value = packet.value;
                    displayName = $"{varName} = {value}";
                    RefreshLabel();
                    player.ConsumeCube();
                }
            }
        }
        else if (varName != "???" && value != "???")
        {
            SpawnValueCube();
        }
    }

    // Genera un cub amb el valor actual del slot de RAM.
    private void SpawnValueCube()
    {
        GameObject newCube = Instantiate(cubePrefab, spawnPoint.position, Quaternion.identity);
        newCube.GetComponent<DataPacket>().SetData(value, false);
        Debug.Log($"RAM: Has extret el valor {value} de la variable {varName}");
    }
}