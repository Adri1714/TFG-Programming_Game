using System.Collections.Generic;
using UnityEngine;

public class TaskGuideManager : MonoBehaviour
{
    private static readonly List<GuidedDevice> devices = new List<GuidedDevice>();

    public static void Register(GuidedDevice d) { if (!devices.Contains(d)) devices.Add(d); }
    public static void Unregister(GuidedDevice d) => devices.Remove(d);

    private void Start()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnTaskStateChanged += ApplyGuide;
        ApplyGuide(GameManager.Instance.currentTaskState);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnTaskStateChanged -= ApplyGuide;
    }

    private void ApplyGuide(GameManager.TaskState state)
    {
        foreach (var device in devices)
            if (device != null) device.SetGuided(IsRelevant(state, device.Category));
    }

    private bool IsRelevant(GameManager.TaskState state, DeviceCategory category)
    {
        bool needsAlu = GameManager.Instance != null && GameManager.Instance.CurrentTaskNeedsAlu;

        switch (state)
        {
            case GameManager.TaskState.DIM_MEM:
            case GameManager.TaskState.WRITE_MEM:
                return category == DeviceCategory.CodeMemory
                    || category == DeviceCategory.WorkMemory
                    || (category == DeviceCategory.ALU && needsAlu);
            case GameManager.TaskState.WRITE_IO:
                return category == DeviceCategory.Output
                    || (category == DeviceCategory.ALU && needsAlu);
            case GameManager.TaskState.PRESS_JMP:
                return category == DeviceCategory.JumpUnit;
            default:
                return false;
        }
    }
}
