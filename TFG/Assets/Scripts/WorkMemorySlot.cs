using UnityEngine;

public class WorkMemorySlot : MonoBehaviour
{
    public string varName = "???";
    public string value = "???";
    public bool isInitialized => varName != "???";

    public void WriteData(DataPacket packet)
    {
        if (packet.isIdentifier) {
            varName = packet.value;
        } else {
            value = packet.value; 
        }
    }
}