using UnityEngine;

// Gestiona la carrega i neteja dels slots de memoria de codi (ROM).
public class CodeMemoryManager : MonoBehaviour 
{
    public CodeMemorySlot[] codeSlots;

    private void Awake()
    {
        // Auto-detectem els slots fills si no s'han assignat a l'Inspector
        if (codeSlots == null || codeSlots.Length == 0)
        {
            codeSlots = GetComponentsInChildren<CodeMemorySlot>();
            Debug.Log($"CodeMemoryManager: Detectats {codeSlots.Length} slots de ROM.");
        }
    }

    // Escriu una instruccio en un slot i defineix el text que es mostrara.
    public void LoadInstructionData(int slotIndex, string value, bool isID, string customDisplayName = null) 
    {
        if (slotIndex < 0 || slotIndex >= codeSlots.Length) 
        {
            Debug.LogError($"[CodeMemory] ERROR: Intentant accedir al slot {slotIndex}, però només n'hi ha {codeSlots.Length}. Duplica més slots a l'escena!");
            return;
        }
        
        if (codeSlots[slotIndex] == null) return;

        string shownName = string.IsNullOrWhiteSpace(customDisplayName) ? value : customDisplayName;
        codeSlots[slotIndex].SetReadOnlyData(shownName, value, isID);

        Debug.Log($"Codi: Carregat '{shownName}' (raw: '{value}', isID: {isID}) al slot {slotIndex}");
    }

    // Sobrecarregues per compatibilitat amb crides existents.
    public void SpawnInstructionData(int slotIndex, string value, bool isID)
    {
        LoadInstructionData(slotIndex, value, isID);
    }

    public void SpawnInstructionData(int slotIndex, string value, bool isID, string customDisplayName)
    {
        LoadInstructionData(slotIndex, value, isID, customDisplayName);
    }

    // Deixa el slot en estat buit/indefinit.
    public void ClearSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= codeSlots.Length) return;
        if (codeSlots[slotIndex] == null) return;

        codeSlots[slotIndex].SetReadOnlyData("???", "???", false);
    }
}