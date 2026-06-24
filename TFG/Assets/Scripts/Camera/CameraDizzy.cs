using System.Collections;
using UnityEngine;

// Efecte de "mareig": balanceig suau de la càmera (rotació + lleuger desplaçament).
// Anàleg a CameraShake, però amb moviment ondulant en lloc de vibració aleatòria.
public class CameraDizzy : MonoBehaviour
{
    [Header("Rotació (graus)")]
    [SerializeField] private float rollAmplitude = 6f;   // balanceig lateral (Z)
    [SerializeField] private float pitchAmplitude = 3f;  // amunt/avall (X)
    [SerializeField] private float yawAmplitude = 3f;    // esquerra/dreta (Y)

    [Header("Desplaçament")]
    [SerializeField] private float positionAmplitude = 0.1f;

    [Header("Velocitat i suavitat")]
    [SerializeField] private float speed = 1.5f;
    [SerializeField, Min(0f)] private float fadeInTime = 0.6f;
    [SerializeField, Min(0.01f)] private float fadeOutTime = 1f;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Coroutine dizzyRoutine;

    private void Awake()
    {
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
    }

    // Marea la càmera durant 'duration' segons amb una intensitat donada (1 = normal).
    public void Dizzy(float duration, float intensity = 1f)
    {
        if (dizzyRoutine != null) StopCoroutine(dizzyRoutine);
        dizzyRoutine = StartCoroutine(DizzyRoutine(duration, intensity));
    }

    // Atura el mareig de manera suau abans d'hora.
    public void StopDizzy()
    {
        if (dizzyRoutine != null) StopCoroutine(dizzyRoutine);
        dizzyRoutine = StartCoroutine(FadeOutAndRestore());
    }

    private IEnumerator DizzyRoutine(float duration, float intensity)
    {
        float elapsed = 0f;
        float seed = Random.value * 10f;   // desfasament perquè no comenci sempre igual

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Envelope: puja al principi (fadeIn) i baixa al final (fadeOut).
            float env = Mathf.Min(
                fadeInTime > 0f ? elapsed / fadeInTime : 1f,
                (duration - elapsed) / fadeOutTime);
            env = Mathf.Clamp01(env) * intensity;

            ApplySway(elapsed + seed, env);
            yield return null;
        }

        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
        dizzyRoutine = null;
    }

    private void ApplySway(float t, float env)
    {
        // Diferents freqüències per a un balanceig orgànic (no sincronitzat).
        float roll  = Mathf.Sin(t * speed)              * rollAmplitude;
        float pitch = Mathf.Sin(t * speed * 0.7f + 1.3f) * pitchAmplitude;
        float yaw   = Mathf.Sin(t * speed * 0.5f + 2.1f) * yawAmplitude;

        transform.localRotation = originalLocalRotation * Quaternion.Euler(pitch * env, yaw * env, roll * env);

        Vector3 offset = new Vector3(
            Mathf.Sin(t * speed * 0.6f),
            Mathf.Sin(t * speed * 0.9f + 0.5f),
            0f) * (positionAmplitude * env);
        transform.localPosition = originalLocalPosition + offset;
    }

    private IEnumerator FadeOutAndRestore()
    {
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;
        float t = 0f;

        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float k = t / fadeOutTime;
            transform.localPosition = Vector3.Lerp(startPos, originalLocalPosition, k);
            transform.localRotation = Quaternion.Slerp(startRot, originalLocalRotation, k);
            yield return null;
        }

        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
        dizzyRoutine = null;
    }
}