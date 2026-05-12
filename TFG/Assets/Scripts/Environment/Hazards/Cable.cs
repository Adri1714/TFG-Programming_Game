using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Cable : MonoBehaviour, Hazard
{
    [SerializeField] private float chargeInterval = 3f;
    [SerializeField] private float slowDuration = 2f;
    [SerializeField, Range(0.1f, 1f)] private float slowMultiplier = 0.5f;

    private PlayerController currentPlayer;
    private Coroutine chargeRoutine;
    private Coroutine slowRoutine;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PlayerController player)) return;

        currentPlayer = player;

        if (chargeRoutine == null)
            chargeRoutine = StartCoroutine(ChargeLoop());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out PlayerController player)) return;
        if (player != currentPlayer) return;

        currentPlayer = null;

        if (chargeRoutine != null)
        {
            StopCoroutine(chargeRoutine);
            chargeRoutine = null;
        }
    }

    private IEnumerator ChargeLoop()
    {
        var wait = new WaitForSeconds(chargeInterval);

        while (currentPlayer != null)
        {
            TriggerHazard(currentPlayer);
            yield return wait;
        }

        chargeRoutine = null;
    }

    public void TriggerHazard(PlayerController player)
    {
        if (player == null) return;

        player.DropCube();

        if (slowRoutine != null)
            StopCoroutine(slowRoutine);

        slowRoutine = StartCoroutine(SlowPlayer(player));
    }

    private IEnumerator SlowPlayer(PlayerController player)
    {
        float originalSpeed = player.moveSpeed;
        player.moveSpeed = originalSpeed * slowMultiplier;

        yield return new WaitForSeconds(slowDuration);

        if (player != null)
            player.moveSpeed = originalSpeed;

        slowRoutine = null;
    }
}