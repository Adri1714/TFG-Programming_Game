using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Rat : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float lifeTime = 12f;
    [SerializeField] private bool destroyOnTouch = false;
    [SerializeField] private float wobble = 25;

    private Vector3 direction = Vector3.forward;
    private PanicHazard panicHazard;
    private bool triggered;

    public void Init(Vector3 dir, PanicHazard hazard)
    {
        panicHazard = hazard;
        direction = dir.sqrMagnitude > 0.001f ? dir.normalized : Vector3.forward;
        transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, -90f, 0f);
    }

    private void Start()
    {
        if (panicHazard == null) panicHazard = FindAnyObjectByType<PanicHazard>();
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        transform.rotation *= Quaternion.Euler(0f, Mathf.Sin(Time.time * wobble), 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.TryGetComponent(out PlayerController _)) return;

        triggered = true;
        if (panicHazard != null) panicHazard.TriggerPanic();
        if (destroyOnTouch) Destroy(gameObject);
    }
}
