using System.Collections;
using UnityEngine;

// Fa aparexier un hazard en punts de spawn del nivell cada cert temps.
public class HazardSpawner : MonoBehaviour
{
    [Header("Hazard")]
    [SerializeField] private GameObject hazardPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Timing")]
    [Tooltip("Temps inicial d'espera abans del primer spawn.")]
    [SerializeField, Min(0f)] private float initialDelay = 1f;

    [Tooltip("Si actiu, l'interval entre spawns és aleatori dins del rang.")]
    [SerializeField] private bool randomizeInterval = true;

    [SerializeField] private Vector2 intervalRange = new Vector2(6f, 14f);

    [Tooltip("Interval fix si randomizeInterval és fals.")]
    [SerializeField, Min(0.1f)] private float fixedInterval = 10f;

    [Header("Spawn Position")]
    [Tooltip("Si actiu, escull un punt de spawn aleatori cada vegada.")]
    [SerializeField] private bool randomizePosition = true;

    [Tooltip("Evita repetir el mateix punt de spawn dos cops seguits.")]
    [SerializeField] private bool avoidRepeatPosition = true;
    [SerializeField] private bool updatePosition = true;

    private GameObject activeInstance;
    private Coroutine spawnRoutine;
    private int lastSpawnIndex = -1;

    private void OnEnable()
    {
        if (hazardPrefab == null)
        {
            Debug.LogWarning($"[HazardSpawner] Falta hazardPrefab a {gameObject.name}.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning($"[HazardSpawner] No hi ha spawnPoints a {gameObject.name}.");
            return;
        }

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        DestroyActiveInstance();
    }

    private IEnumerator SpawnLoop()
    {
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            SpawnAtNextPoint();
            if (!updatePosition) yield break;

            float delay = randomizeInterval
                ? Random.Range(intervalRange.x, intervalRange.y)
                : fixedInterval;

            yield return new WaitForSeconds(delay);
        }
    }

    private void SpawnAtNextPoint()
    {
        DestroyActiveInstance();

        Transform point = GetNextSpawnPoint();
        if (point == null) return;

        activeInstance = Instantiate(hazardPrefab, point.position, point.rotation);
    }

    // Tria el seguent punt segons configuracio (aleatori o seqüencial).
    private Transform GetNextSpawnPoint()
    {
        if (spawnPoints.Length == 1) return spawnPoints[0];

        int index;

        if (randomizePosition)
        {
            if (avoidRepeatPosition && spawnPoints.Length > 1)
            {
                do { index = Random.Range(0, spawnPoints.Length); }
                while (index == lastSpawnIndex);
            }
            else
            {
                index = Random.Range(0, spawnPoints.Length);
            }
        }
        else
        {
            index = (lastSpawnIndex + 1) % spawnPoints.Length;
        }

        lastSpawnIndex = index;
        return spawnPoints[index];
    }

    private void DestroyActiveInstance()
    {
        if (activeInstance != null)
        {
            Destroy(activeInstance);
            activeInstance = null;
        }
    }
}
