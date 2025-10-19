using UnityEngine;

[System.Serializable]
public class Optional<T>
{
    public bool enabled;
    public T value;
    
    public Optional(T initValue)
    {
        enabled = true;
        value = initValue;
    }
    
    public Optional()
    {
        enabled = false;
    }
    
    public static implicit operator T(Optional<T> optional)
    {
        return optional.value;
    }
}

[System.Serializable]
public struct Property<T> : ISerializationCallbackReceiver where T : System.Enum
{
    public string[] serializedEnumNames;
    public ulong properties;
    
    private string[] enumNames => System.Enum.GetNames(typeof(T));
    
    public Property(params T[] properties) : this()
    {
        string[] names = enumNames;
        GameDebug.Assert(names.Length <= sizeof(ulong) * 8, "Enum: " + names.Length);
        GameDebug.Assert(properties.Length <= names.Length, "Properties: " + properties.Length);
        SetProperties(properties);
    }
    
    public static implicit operator Property<T>(T flags)
    {
        return new Property<T>(flags);
    }
    
    public bool HasProperty(T property)
    {
        int p = System.Convert.ToInt32(property);
        return (properties & (1ul << p)) != 0;
    }
    
    public void SetProperty(T property, bool set)
    {
        int p = System.Convert.ToInt32(property);
        properties = MathUtils.SetFlag(properties, p, set);
    }
    
    public void SetProperties(params T[] properties)
    {
        if (properties != null)
            foreach (T property in properties)
                SetProperty(property, true);
    }
    
    public override string ToString()
    {
        return "Enum: " + typeof(T) + " Properties: " + properties;
    }
    
    // NOTE: OnBeforeSerialize will get called before the code is compiled and OnAfterDeserialize will get called afterward.
    // The inspector will call OnBeforeSerialize multiple times then call OnGUI but never OnAfterDeserialize.
    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize()
    {
        string[] enumNames = this.enumNames;
        Debug.Assert(enumNames.Length <= sizeof(ulong) * 8, "Enum " + typeof(T).FullName + " has " + enumNames.Length);
        Debug.Assert(enumNames.Length > 0, "Enum " + typeof(T).FullName);
        
        if (serializedEnumNames == null || serializedEnumNames.Length == 0)
            serializedEnumNames = enumNames;
        
        bool[] sets = new bool[enumNames.Length];
        for (int i = 0; i < enumNames.Length; i++)
        {
            bool currentValue = MathUtils.HasFlag(properties, i);
            string currentName = enumNames[i];
            if (i >= serializedEnumNames.Length || currentName != serializedEnumNames[i])
            {
                bool findOldIndex = false;
                for (int oldIndex = 0; oldIndex < serializedEnumNames.Length; oldIndex++)
                {
                    if (serializedEnumNames[oldIndex] == currentName)
                    {
                        currentValue = MathUtils.HasFlag(properties, oldIndex);
                        findOldIndex = true;
                        if (currentValue)
                            Debug.Log("New index for " + serializedEnumNames[oldIndex] + " is " + i);
                        break;
                    }
                }
                
                if (!findOldIndex)
                {
                    string msg = "Can't find old value for " + currentName + " at " + i;
                    if (i < serializedEnumNames.Length)
                        msg += ". Old name is " + serializedEnumNames[i];
                    if (currentValue) Debug.LogWarning(msg);
                    //else              Debug.Log(msg);
                    currentValue = false;
                }
            }
            sets[i] = currentValue;
        }
        
        for (int i = 0; i < sets.Length; i++)
            properties = MathUtils.SetFlag(properties, i, sets[i]);
        
        serializedEnumNames = enumNames;
    }
}

[System.Serializable]
public struct RangedInt
{
    [Tooltip("Min Inclusive")]
    public int min;
    [Tooltip("Max Inclusive")]
    public int max;
    
    public RangedInt(int min, int max)
    {
        this.min = min;
        this.max = max;
    }
    
    public int randomValue => Random.Range(min, max + 1);
}

[System.Serializable]
public struct RangedFloat
{
    [Tooltip("Min Inclusive")]
    public float min;
    [Tooltip("Max Inclusive")]
    public float max;
    
    public RangedFloat(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
    
    public RangedFloat(float value)
    {
        this.min = this.max = value;
    }
    
    public RangedFloat(System.Array array)
    {
        this.min = 0;
        this.max = array.Length - 1;
    }
    
    public float randomValue
    {
        get
        {
            if (min == max)
                return min;
            return Random.Range(min, max);
        }
    }
    
    public float range => max - min;
    
    public bool InRange(float value) => value >= min && value <= max;
}

[System.Serializable]
public class IntReference
{
    public bool useConstant = true;
    public int constantValue;
    public IntVariable variable;
    
    public int value
    {
        get => useConstant ? constantValue : variable.value;
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
