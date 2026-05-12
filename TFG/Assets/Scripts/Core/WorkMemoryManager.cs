using UnityEngine;
using System.Collections.Generic;

// Registre central dels slots de RAM disponibles al nivell.
public class WorkMemoryManager : MonoBehaviour
{
    public List<WorkMemorySlot> ramSlots = new List<WorkMemorySlot>();

    private void Awake()
    {
        // Auto-detectem tots els calaixos que pengen d'aquest objecte
        if (ramSlots.Count == 0)
        {
            ramSlots = new List<WorkMemorySlot>(GetComponentsInChildren<WorkMemorySlot>());
            Debug.Log($"WorkMemoryManager: Detectats {ramSlots.Count} slots de RAM.");
        }
    }

    // Retorna el valor de la variable si existeix en algun slot.
    public string GetVariableValue(string varName)
    {
        foreach (var slot in ramSlots)
        {
            if (slot.varName.Trim() == varName.Trim()) return slot.value;
        }
        return null;
    }
}