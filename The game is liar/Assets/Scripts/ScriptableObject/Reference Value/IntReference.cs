using UnityEngine;

[System.Serializable]
public class IntReference
{
    public FieldType type;
    [ShowWhen("type", FieldType.Constant)] public int constantValue;
    [ShowWhen("type", FieldType.Reference)] public IntVariable variable;
    [ShowWhen("type", FieldType.Random)] public Vector2Int valueRange;

    public int value
    {
        get
        {
            switch (type)
            {
                case FieldType.Reference:
                    return variable.value;
                case FieldType.Random:
                    return Random.Range(valueRange.x, valueRange.y + 1);
                default:
                    return constantValue;
            }
        }
        set
        {
            if (type == FieldType.Constant)
                constantValue = value;
            else if (type == FieldType.Reference)
                variable.value = value;
        }
    }

    public IntReference(int value)
    {
        constantValue = value;
        type = FieldType.Constant;
    }

    public static implicit operator int(IntReference reference)
    {
        return reference.value;
    }
}
