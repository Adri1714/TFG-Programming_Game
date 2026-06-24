using UnityEngine;

public class InteractableHighlight : MonoBehaviour
{
    public Renderer targetRenderer;
    public Color highlightColor = Color.yellow;

    [Header("Guia (glow pulsant)")]
    public Color guideColor = Color.cyan;
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float minEmission = 0.2f;
    [SerializeField] private float maxEmission = 1.8f;

    private Color originalColor;
    private bool isHighlighted;
    private bool isGuided;
    private Material mat;
    private bool hasEmission;

    void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
        if (targetRenderer != null)
        {
            mat = targetRenderer.material;
            originalColor = mat.color;
            hasEmission = mat.HasProperty("_EmissionColor");
        }
    }

    void Update()
    {
        if (!isGuided || mat == null || !hasEmission) return;

        float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        mat.SetColor("_EmissionColor", guideColor * Mathf.Lerp(minEmission, maxEmission, t));
    }

    public void SetHighlight(bool state)
    {
        if (mat == null || isHighlighted == state) return;
        isHighlighted = state;
        mat.color = state ? highlightColor : originalColor;
    }

    public void SetGuided(bool state)
    {
        if (mat == null || isGuided == state) return;
        isGuided = state;
        if (!hasEmission) return;

        if (state)
        {
            mat.EnableKeyword("_EMISSION");
        }
        else
        {
            mat.SetColor("_EmissionColor", Color.black);
            mat.DisableKeyword("_EMISSION");
        }
    }
}
