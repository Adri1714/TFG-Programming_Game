using UnityEngine;
using System.Collections.Generic;

// Registre central dels slots de RAM disponibles al nivell.
public class WorkMemoryManager : MonoBehaviour
{
    public List<WorkMemorySlot> ramSlots = new List<WorkMemorySlot>();

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