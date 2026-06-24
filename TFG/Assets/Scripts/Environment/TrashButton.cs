using UnityEngine;

public class TrashButton : MonoBehaviour, IInteractable
{
    public void HandleInteraction(PlayerController player)
    {
        AudioManager.Play(l => l.trash);
        GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube");
        foreach (GameObject cube in cubes)
        {
            if (cube == player.carriedCube) continue;
            Destroy(cube);
        }
    }
}
