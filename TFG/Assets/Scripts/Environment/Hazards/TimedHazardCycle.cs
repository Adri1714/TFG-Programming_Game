using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedHazardCycle : MonoBehaviour
{
    [SerializeField] private float cycleDuration = 5f;
    [SerializeField] private float activeDuration = 3f;
    [SerializeField] private float triggerInterval = 0.5f;
    [SerializeField] private bool randomizeInactiveDuration = true;
    [SerializeField] private Vector2 inactiveDurationRange = new Vector2(1f, 4f);
    [SerializeField] private bool triggerOnActiveStart = true;
    [SerializeField] private bool triggerWhenPlayerEntersDuringActive = true;

    private Coroutine cycleRoutine;
    private readonly List<IHazardEffect> hazards = new List<IHazardEffect>();
    private PlayerController currentPlayer;
    private bool isActive;

    private void Awake()
    {
        hazards.Clear();

        foreach (MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
        {
            if (behaviour is IHazardEffect hazardEffect)
            {
                hazards.Add(hazardEffect);
            }
        }
    }

    private void OnEnable()
    {
        cycleRoutine = StartCoroutine(HazardCycle());
    }

    private void OnDisable()
    {
        if (cycleRoutine != null)
        {
            StopCoroutine(cycleRoutine);
            cycleRoutine = null;
        }

        isActive = false;
        currentPlayer = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            currentPlayer = player;

            if (isActive && triggerWhenPlayerEntersDuringActive)
            {
                TriggerHazards(currentPlayer);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out PlayerController player)) return;
        if (player == currentPlayer)
        {
            currentPlayer = null;
        }
    }
    private void SetCycleVisuals(bool active)
    {
        foreach (MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
        {
            if (behaviour is Cable cable)
            {
                cable.SetHazardActive(active);
            }
        }
    }
    private IEnumerator HazardCycle()
    {
        if (cycleDuration > 0f)
        {
            yield return new WaitForSeconds(Random.Range(0f, cycleDuration));
        }

        while (true)
        {
            isActive = true;
            SetCycleVisuals(true);
            float activeInterval = Mathf.Max(0.01f, triggerInterval);
            float activeTime = 0f;

            if (triggerOnActiveStart && currentPlayer != null)
            {
                TriggerHazards(currentPlayer);
            }

            WaitForSeconds activeWait = new WaitForSeconds(activeInterval);

            while (activeTime < activeDuration)
            {
                yield return activeWait;
                activeTime += activeInterval;

                if (activeTime < activeDuration && currentPlayer != null)
                {
                    TriggerHazards(currentPlayer);
                }
            }

            SetCycleVisuals(false);
            isActive = false;

            float inactiveDuration = GetInactiveDuration();

            if (inactiveDuration > 0f)
            {
                yield return new WaitForSeconds(inactiveDuration);
            }
        }
    }

    private float GetInactiveDuration()
    {
        if (!randomizeInactiveDuration)
        {
            return Mathf.Max(0f, cycleDuration - activeDuration);
        }

        float minDuration = Mathf.Max(0f, inactiveDurationRange.x);
        float maxDuration = Mathf.Max(minDuration, inactiveDurationRange.y);
        return Random.Range(minDuration, maxDuration);
    }

    private void TriggerHazards(PlayerController player)
    {
        if (player == null || hazards.Count == 0)
        {
            return;
        }

        for (int i = 0; i < hazards.Count; i++)
        {
            hazards[i].TriggerHazard(player);
        }
    }
}