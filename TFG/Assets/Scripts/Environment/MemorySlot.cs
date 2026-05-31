using UnityEngine;
using TMPro;

public abstract class MemorySlot : MonoBehaviour, IInteractable
{
    public string varName = "???";
    public string value = "???";
    public string displayName = "???";
    public TMP_Text labelText;

    public virtual void SetVariable(string name, string val)
    {
        varName = name;
        value = val;
        displayName = name;
        RefreshLabel();
    }

    protected void RefreshLabel()
    {
        if (labelText != null)
            labelText.text = displayName;
    }

    public abstract void HandleInteraction(PlayerController player);
}
