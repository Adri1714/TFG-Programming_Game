using UnityEngine;

[DisallowMultipleComponent]
public class ReverseControlsHazard : TimedPlayerEffectHazard
{
    [SerializeField] private CameraDizzy cameraDizzy;
    [SerializeField] private float dizzyDuration = 4f;
    [SerializeField] private float dizzyIntensity = 1f;

    private void Awake()
    {
        if (cameraDizzy == null)
            cameraDizzy = Camera.main.GetComponent<CameraDizzy>();
    }
    private const string EffectSource = nameof(ReverseControlsHazard);

    protected override void ApplyEffect(PlayerController player)
    {
        if (player == null) return;
        if(player.playerControlsReversed) return;
        AudioManager.Instance.ApplyEffectLowPass();
        cameraDizzy.Dizzy(dizzyDuration, dizzyIntensity);
        player.SetControlsReversed(EffectSource, true);
    }

    protected override void RemoveEffect(PlayerController player)
    {
        if (player == null) return;

        player.ClearControlsReversal(EffectSource);
        AudioManager.Instance.RemoveEffectLowPass();
        player.playerControlsReversed = false;
    }
}