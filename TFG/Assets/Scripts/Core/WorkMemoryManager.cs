using UnityEngine;
using System;
using System.Collections.Generic;

public class WorkMemoryManager : MonoBehaviour
{
    [SerializeField] private List<WorkMemorySlot> ramSlots = new();
    private Dictionary<string, WorkMemorySlot> _slotMap;

    private void Awake()
    {
        var slots = GetComponentsInChildren<WorkMemorySlot>(true);
        ramSlots.Clear();
        ramSlots.AddRange(slots);

        _slotMap = new Dictionary<string, WorkMemorySlot>(slots.Length, StringComparer.Ordinal);

        foreach (var slot in slots)
        {
            if (slot == null || string.IsNullOrWhiteSpace(slot.varName))
                continue;

            _slotMap[slot.varName.Trim()] = slot;
        }
    }

    public string GetVariableValue(string varName)
    {
        if (string.IsNullOrWhiteSpace(varName))
            return null;

        var key = varName.Trim();
        return _slotMap != null && _slotMap.TryGetValue(key, out var slot) ? slot.value : null;
    }
}