[System.Serializable]
public class FloatReference
{
    public FieldType fieldType;
    [ShowWhen("fieldType", FieldType.Constant)] public float constantValue;
    [ShowWhen("fieldType", FieldType.Reference)] public FloatVariable variable;
    [ShowWhen("fieldType", FieldType.Random, order = 0)] public RangedFloat valueRange;

    public float value
    {
        get
        {
            switch (fieldType)
            {
                case FieldType.Reference:
                    return variable.value;
                case FieldType.Random:
                    return valueRange.randomValue;
                default:
                    return constantValue;
            }
        }
        set
        {
            if (fieldType == FieldType.Constant)
                constantValue = value;
            else if (fieldType == FieldType.Reference)
                variable.value = value;
        }
    }

    public FloatReference(float value)
    {
        constantValue = value;
        fieldType = FieldType.Constant;
    }

    public static implicit operator float(FloatReference reference)
    {
        return reference.value;
    }
}
