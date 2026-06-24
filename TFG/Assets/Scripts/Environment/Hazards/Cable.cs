using UnityEngine;

[DisallowMultipleComponent]
public class Cable : TimedPlayerEffectHazard
{
    [SerializeField, Range(0.1f, 1f)] private float slowMultiplier = 0.5f;
    [SerializeField] private bool dropCubeOnTrigger = true;
    [SerializeField] private GameObject sparkEffect;
    [SerializeField] private Transform sparkEffectSpawnPoint;
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private float cameraShakeDuration = 0.2f;
    [SerializeField] private float cameraShakeMagnitude = 0.12f;
    private GameObject activeSparkInstance;
    private const string ModifierSource = nameof(Cable);

    void Start()
    {
        if (cameraShake == null) cameraShake = Camera.main.GetComponent<CameraShake>();
    }

    protected override void ApplyEffect(PlayerController player)
    {
        if (player == null) return;
        AudioManager.Play(l => l.shock);
        if (dropCubeOnTrigger) player.DropCube();
        cameraShake.Shake(cameraShakeDuration, cameraShakeMagnitude);
        player.SetMoveSpeedMultiplier(ModifierSource, slowMultiplier);
    }

    protected override void RemoveEffect(PlayerController player)
    {
        if (player == null) return;
        player.ClearMoveSpeedMultiplier(ModifierSource);
    }

    public void SetHazardActive(bool active)
    {
        if (active)
        {
            if (activeSparkInstance == null && sparkEffect != null && sparkEffectSpawnPoint != null)
                activeSparkInstance = Instantiate(sparkEffect, sparkEffectSpawnPoint.position, sparkEffectSpawnPoint.rotation, sparkEffectSpawnPoint);
        }
        else if (activeSparkInstance != null)
        {
            Destroy(activeSparkInstance);
            activeSparkInstance = null;
        }
    }
}
