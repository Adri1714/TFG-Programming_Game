using UnityEngine;

// Ressalt visual per indicar quin objecte es pot interactuar.
public class InteractableHighlight : MonoBehaviour
{
    public Renderer targetRenderer;
    public Color highlightColor = Color.yellow;
    
    private Color originalColor;
    private bool isHighlighted = false;

    // Desa el color inicial per restaurar-lo en desactivar el highlight.
    void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
        if (targetRenderer != null) originalColor = targetRenderer.material.color;
    }

    // Canvia entre color base i color de ressalt.
    public void SetHighlight(bool state)
    {
        if (targetRenderer == null || isHighlighted == state) return;
        
        isHighlighted = state;
        targetRenderer.material.color = state ? highlightColor : originalColor;
    }
}