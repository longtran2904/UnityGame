[System.Serializable]
public class IntReference
{
    public bool useConstant = true;
    public int constantValue;
    public IntVariable variable;

    public int value
    {
        get
        {
            return useConstant ? constantValue : variable.value;
        }
        set
        {
            if (useConstant)
                constantValue = value;
            else
                variable.value = value;
        }
    }

    public IntReference(int value)
    {
        useConstant = true;
        constantValue = value;
    }

    public static implicit operator int(IntReference reference)
    {
        return reference.value;
    }
}
