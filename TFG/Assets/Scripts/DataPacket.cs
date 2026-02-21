using UnityEngine;

public class DataPacket : MonoBehaviour
{
    public string value;
    public bool isIdentifier;

    public void SetData( string newValue, bool identifier = false)
    {
        value = newValue;
        isIdentifier = identifier;
    }
}
