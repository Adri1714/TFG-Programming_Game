using TMPro;
using UnityEngine;

public class DataPacket : MonoBehaviour
{
    public string value;
    public bool isIdentifier;
    public TMP_Text displayText;
    public bool fromAlu;
    public bool fromWorkMem;

    public void SetData(string val, bool id, bool computed = false, bool fromWorkMemory = false)
    {
        value = val;
        isIdentifier = id;
        fromAlu = computed;
        fromWorkMem = fromWorkMemory;
        if (displayText != null) displayText.text = value;
    }
}
