using UnityEngine;

[System.Serializable]
public class Optional<T>
{
    [SerializeField] private bool enabled;
    [SerializeField] private T value;

    public bool Enabled => enabled;
    public T Value => value;

    public Optional(T initValue)
    {
        enabled = true;
        value = initValue;
    }

    public static implicit operator T(Optional<T> optional)
    {
        return optional.value;
    }
}
