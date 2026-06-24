using System.Collections;
using UnityEngine;

public abstract class TimedPlayerEffectHazard : MonoBehaviour, IHazardEffect
{
    [SerializeField, Min(0f)] private float effectDuration = 2f;
    [SerializeField] private bool clearEffectOnDisable = true;

    private PlayerController activePlayer;
    private Coroutine effectRoutine;

    public void TriggerHazard(PlayerController player)
    {
        if (player == null) return;
        ActivateEffect(player);
    }

    protected void ActivateEffect(PlayerController player)
    {
        if (activePlayer != null && activePlayer != player)
            ClearActiveEffect();

        activePlayer = player;
        ApplyEffect(player);

        if (effectRoutine != null)
            StopCoroutine(effectRoutine);

        if (effectDuration <= 0f)
        {
            ClearActiveEffect();
            return;
        }

        effectRoutine = StartCoroutine(ClearEffectAfterDelay(player));
    }

    protected abstract void ApplyEffect(PlayerController player);

    protected abstract void RemoveEffect(PlayerController player);

    protected virtual void OnDisable()
    {
        if (clearEffectOnDisable)
            ClearActiveEffect();
    }

    private IEnumerator ClearEffectAfterDelay(PlayerController player)
    {
        yield return new WaitForSeconds(effectDuration);

        if (player != null)
            RemoveEffect(player);

        if (activePlayer == player)
            activePlayer = null;

        effectRoutine = null;
    }

    private void ClearActiveEffect()
    {
        if (effectRoutine != null)
        {
            StopCoroutine(effectRoutine);
            effectRoutine = null;
        }

        if (activePlayer != null)
        {
            RemoveEffect(activePlayer);
            activePlayer = null;
        }
    }
}
