using UnityEngine;

[DisallowMultipleComponent]
public class ReverseControlsHazard : TimedPlayerEffectHazard
{
    private const string EffectSource = nameof(ReverseControlsHazard);

    protected override void ApplyEffect(PlayerController player)
    {
        if (player == null) return;

        player.SetControlsReversed(EffectSource, true);
    }

    protected override void RemoveEffect(PlayerController player)
    {
        if (player == null) return;

        player.ClearControlsReversal(EffectSource);
    }
}