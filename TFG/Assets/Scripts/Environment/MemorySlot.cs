using UnityEngine;
using TMPro;

// Classe base per a slots de memoria interactuables.
public abstract class MemorySlot : MonoBehaviour
{
    public string varName = "???";
    public string value = "???";
    public string displayName = "???";
    public TMP_Text labelText;

    // Assigna variable/valor i refresca el text visible del slot.
    public virtual void SetVariable(string name, string val)
    {
        varName = name;
        value = val;
        displayName = name;
        RefreshLabel();
    }

    // Actualitza la etiqueta 3D/WorldSpace associada al slot.
    protected void RefreshLabel()
    {
        if (labelText != null)
            labelText.text = displayName;
    }

    // Implementacio especifica segons el tipus de slot.
    public abstract void HandleInteraction(PlayerController player);
}