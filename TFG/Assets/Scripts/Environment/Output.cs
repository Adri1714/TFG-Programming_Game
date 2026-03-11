using UnityEngine;

// Punt de sortida: envia el valor del cub a STDOUT si la tasca ho demana.
public class Output : MonoBehaviour
{
    // Consumeix el cub nomes quan l'accio esperada es correcta.
    public void HandleInteraction(PlayerController player)
    {
        if (player.carriedCube == null) return;

        DataPacket packet = player.carriedCube.GetComponent<DataPacket>();

        if (GameManager.Instance.ValidateAction(GameManager.TaskState.WRITE_IO, "STDOUT", packet.value))
        {
            player.ConsumeCube();
            Debug.Log("Dada enviada correctament a STDOUT.");
        }
    }
}