using System.Collections;
using UnityEngine;

public class RatSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ratPrefab;
    [SerializeField] private Transform[] entryPoints;
    [Tooltip("Opcional. Si n'hi ha, la rata va d'una entrada cap a una sortida; si no, usa el forward de l'entrada.")]
    [SerializeField] private Transform[] exitPoints;
    [SerializeField] private Vector2 intervalRange = new Vector2(4f, 9f);
    [SerializeField, Min(0f)] private float initialDelay = 3f;
    [SerializeField] private PanicHazard panicHazard;

    private void OnEnable()
    {
        if (ratPrefab == null || entryPoints == null || entryPoints.Length == 0)
        {
            Debug.LogWarning($"[RatSpawner] Falta ratPrefab o entryPoints a {gameObject.name}.");
            return;
        }
        StartCoroutine(SpawnLoop());
    }

    private void OnDisable() => StopAllCoroutines();

    private IEnumerator SpawnLoop()
    {
        if (initialDelay > 0f) yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            SpawnRat();
            yield return new WaitForSeconds(Random.Range(intervalRange.x, intervalRange.y));
        }
    }

    private void SpawnRat()
    {
        Transform entry = entryPoints[Random.Range(0, entryPoints.Length)];
        Vector3 dir = entry.forward;

        if (exitPoints != null && exitPoints.Length > 0)
        {
            Transform exit = exitPoints[Random.Range(0, exitPoints.Length)];
            dir = exit.position - entry.position;
        }

        GameObject go = Instantiate(ratPrefab, entry.position, Quaternion.identity);
        if (go.TryGetComponent(out Rat rat)) rat.Init(dir, panicHazard);
    }
}
