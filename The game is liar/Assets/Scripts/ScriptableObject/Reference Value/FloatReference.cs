[System.Serializable]
public class FloatReference
{
    public bool useConstant = true;
    [ShowWhen("useConstant")] public float constantValue;
    [ShowWhen("useConstant", false)] public FloatVariable variable;

    public float value
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
