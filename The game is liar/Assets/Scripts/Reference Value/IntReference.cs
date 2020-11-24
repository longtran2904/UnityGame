[System.Serializable]
public class IntReference
{
    public bool useConstant = true;
    [ShowWhen("useConstant")] public int constantValue;
    [ShowWhen("useConstant", false)] public IntVariable variable;

    public int value
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
