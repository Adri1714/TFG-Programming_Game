using UnityEngine;

public class CodeMemoryManager : MonoBehaviour
{
    public CodeMemorySlot[] codeSlots;

    private void Awake()
    {
        if (codeSlots == null || codeSlots.Length == 0)
            codeSlots = GetComponentsInChildren<CodeMemorySlot>();
    }

    // Escriu una instruccio en un slot i defineix el text que mostrara.
    public void LoadInstructionData(int slotIndex, string value, bool isID, string customDisplayName = null)
    {
        if (slotIndex < 0 || slotIndex >= codeSlots.Length)
        {
            Debug.LogError($"[CodeMemory] Slot {slotIndex} fora de rang (n'hi ha {codeSlots.Length}).");
            return;
        }

        if (codeSlots[slotIndex] == null) return;

        string shownName = string.IsNullOrWhiteSpace(customDisplayName) ? value : customDisplayName;
        codeSlots[slotIndex].SetReadOnlyData(shownName, value, isID);
    }

    public void SpawnInstructionData(int slotIndex, string value, bool isID)
    {
        LoadInstructionData(slotIndex, value, isID);
    }

    public void SpawnInstructionData(int slotIndex, string value, bool isID, string customDisplayName)
    {
        LoadInstructionData(slotIndex, value, isID, customDisplayName);
    }

    public void ClearSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= codeSlots.Length) return;
        if (codeSlots[slotIndex] == null) return;

        codeSlots[slotIndex].SetReadOnlyData("???", "???", false);
    }
}
