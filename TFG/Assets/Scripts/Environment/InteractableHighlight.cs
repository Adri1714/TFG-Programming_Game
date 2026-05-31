using UnityEngine;

public class InteractableHighlight : MonoBehaviour
{
    public Renderer targetRenderer;
    public Color highlightColor = Color.yellow;

    private Color originalColor;
    private bool isHighlighted = false;

    void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
        if (targetRenderer != null) originalColor = targetRenderer.material.color;
    }

    public void SetHighlight(bool state)
    {
        if (targetRenderer == null || isHighlighted == state) return;

        isHighlighted = state;
        targetRenderer.material.color = state ? highlightColor : originalColor;
    }
}
