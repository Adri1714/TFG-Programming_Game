using TMPro;
using UnityEngine;

public class DataPacket : MonoBehaviour
{
    public string value;
    public bool isIdentifier;
    public TMP_Text displayText;

    // Actualitza les dades logiques i la representacio visual del cub.
    public void SetData(string val, bool id)
    {
        value = val;
        isIdentifier = id;
        if (displayText != null) displayText.text = value;
    }
}
