using UnityEngine;

public enum DeviceCategory { CodeMemory, WorkMemory, Output, JumpUnit, ALU }

[RequireComponent(typeof(InteractableHighlight))]
public class GuidedDevice : MonoBehaviour
{
    public DeviceCategory category;

    private InteractableHighlight highlight;

    private void Awake() => highlight = GetComponent<InteractableHighlight>();

    private void OnEnable() => TaskGuideManager.Register(this);
    private void OnDisable() => TaskGuideManager.Unregister(this);

    public DeviceCategory Category => category;

    public void SetGuided(bool on)
    {
        if (highlight != null) highlight.SetGuided(on);
    }
}
