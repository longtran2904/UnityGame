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
