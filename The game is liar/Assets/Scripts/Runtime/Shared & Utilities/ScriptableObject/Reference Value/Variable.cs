using System;
using UnityEngine;

public class Variable<T> : ScriptableObject
{
    [HideInInspector] public T value;
    [SerializeField] private T defaultValue;

    // Reset the value after exit play mode
    private void OnEnable()
    {
        value = defaultValue;
    }
}

// NOTE: Before Unity 2020.1, serializing fields of generic types was not possible. So I can't do something like VariableRef<T> and have T variablie.
[Serializable]
public class VariableReference<T>
{
    public bool useConstant = true;
    public T constantValue;
    public Variable<T> variable;

    public T value
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

    public VariableReference(T value)
    {
        useConstant = true;
        constantValue = value;
    }

    public VariableReference()
    {

    }

    public static implicit operator T(VariableReference<T> reference)
    {
        return reference.value;
    }
}