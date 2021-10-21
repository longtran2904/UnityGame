using System;
using UnityEngine;

[Serializable]
public class BoundsIntReference
{
    public bool useConstant = true;
    [ShowWhen("useConstant")] public BoundsInt constantValue;
    [ShowWhen("useConstant", false)] public BoundsIntVariable variable;

    public BoundsInt value
    {
        get { return useConstant ? constantValue : variable.value; }
        set
        {
            if (useConstant)
                constantValue = value;
            else
                variable.value = value;
        }
    }
}
