using UnityEngine;

public class SwirlVisual : MonoBehaviour
{
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 120f, 0f);
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField, Range(0f, 0.5f)] private float pulseAmount = 0.1f;
    [SerializeField, Min(0f)] private float fadeInTime = 0.4f;

    private Vector3 baseScale;
    private float age;

    private void Start() => baseScale = transform.localScale;

    private void Update()
    {
        age += Time.deltaTime;
        transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);

        float grow = fadeInTime > 0f ? Mathf.Clamp01(age / fadeInTime) : 1f;
        float pulse = 1f + Mathf.Sin(age * pulseSpeed * Mathf.PI * 2f) * pulseAmount;
        transform.localScale = baseScale * (grow * pulse);
    }
}
