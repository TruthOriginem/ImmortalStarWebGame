using UnityEngine;
using System.Collections;

public class SerializedIProperty{
    public PROPERTY_TYPE type;
    public float value;
    public SerializedIProperty(PROPERTY_TYPE type,float value)
    {
        this.type = type;
        this.value = value;
    }
}
