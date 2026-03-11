using UnityEngine;

// Slot de memoria de codi en mode nomes lectura.
public class CodeMemorySlot : MemorySlot
{
    public GameObject cubePrefab;
    public Transform spawnPoint;
    public bool isIdentifierData = false;

    // Carrega la dada al slot amb nom visible i valor intern.
    public void SetReadOnlyData(string shownName, string rawValue, bool isIdentifier)
    {
        displayName = shownName;
        value = rawValue;
        isIdentifierData = isIdentifier;
        RefreshLabel();
    }

    // Si el jugador te les mans buides, extreu una copia de la dada.
    public override void HandleInteraction(PlayerController player)
    {
        if (player.carriedCube == null && value != "???")
        {
            SpawnCodeCube();
        }
    }

    // Instancia un cub de lectura amb el contingut del slot.
    private void SpawnCodeCube()
    {
        GameObject newCube = Instantiate(cubePrefab, spawnPoint.position, Quaternion.identity);
        newCube.GetComponent<DataPacket>().SetData(value, isIdentifierData);
        Debug.Log($"CODE: Lectura de slot '{displayName}' -> {value}");
    }
}