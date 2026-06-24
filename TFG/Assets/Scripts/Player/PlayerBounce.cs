using UnityEngine;

public class PlayerBounce : MonoBehaviour
{
    [SerializeField] private float bobHeight = 0.12f;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float stepInterval = 0.35f;
    [SerializeField] private float smooth = 12f;

    private PlayerController player;
    private Vector3 baseLocalPos;
    private float stepTimer;

    private void Start()
    {
        player = GetComponentInParent<PlayerController>();
        baseLocalPos = transform.localPosition;
    }

    private void Update()
    {
        Vector3 target = baseLocalPos;

        if (player != null && player.IsMoving)
        {
            float t = Time.time * speed;
            target.y += Mathf.Abs(Mathf.Sin(t)) * bobHeight;

            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                stepTimer = stepInterval;
                var audioManager = AudioManager.Instance;
                if (audioManager != null && audioManager.Library != null)
                    audioManager.PlaySfx(audioManager.Library.footstep, 0.2f, Random.Range(0.9f, 1.2f));
            }
        }
        else
        {
            stepTimer = 0f;
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * smooth);
    }
}