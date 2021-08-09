using UnityEngine;

[System.Serializable]
public class Vector3Reference
{
    public bool useConstant = true;
    [ShowWhen("useConstant")] public Vector3 constantValue;
    [ShowWhen("useConstant", false)] public Vector3Variable variable;

    public Vector3 value
    {
        get { return useConstant ? constantValue : variable.value; }
    }
}
