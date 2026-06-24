using UnityEngine;

public class CodeMemorySlot : MemorySlot, IInteractable
{
    public GameObject cubePrefab;
    public Transform spawnPoint;
    public bool isIdentifierData = false;

    public void SetReadOnlyData(string shownName, string rawValue, bool isIdentifier)
    {
        displayName = shownName;
        value = rawValue;
        isIdentifierData = isIdentifier;
        RefreshLabel();
    }

    public override void HandleInteraction(PlayerController player)
    {
        if (player.carriedCube == null && value != "???")
            SpawnCodeCube();
    }

    private void SpawnCodeCube()
    {
        GameObject newCube = Instantiate(cubePrefab, spawnPoint.position + new Vector3(0, -1f, 0), Quaternion.identity);
        newCube.GetComponent<DataPacket>().SetData(value, isIdentifierData);
    }
}
