using TMPro;
using UnityEngine;

// Dada transportable dins d'un cub (valor + tipus).
public class DataPacket : MonoBehaviour
{
    public string value;
    public bool isIdentifier;
    public TMP_Text displayText;

    // Actualitza les dades logiques i la seva representacio visual.
    public void SetData(string val, bool id) 
    {   
        value = val; 
        isIdentifier = id; 
        if (displayText != null) displayText.text = value.ToString();
    }
}
