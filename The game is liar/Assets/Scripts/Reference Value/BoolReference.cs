using UnityEngine;

[System.Serializable]
public class BoolReference
{
    public bool useConstant = true;
    [ShowWhen("useConstant")] public bool constantValue;
    [ShowWhen("useConstant", false)] public BoolVariable variable;

    public bool value
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
