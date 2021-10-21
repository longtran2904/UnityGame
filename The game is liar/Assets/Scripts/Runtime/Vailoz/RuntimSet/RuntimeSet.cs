using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RuntimeSet<T> : ScriptableObject
{
    [HideInInspector] public List<T> items;
    [SerializeField] private List<T> defaultItems;

    protected virtual void OnEnable()
    {
        if (defaultItems != null)
            items = new List<T>(defaultItems);
    }

    public void Add(T t)
    {
        if (!items.Contains(t)) items.Add(t);
    }

    public void Remove(T t)
    {
        if (items.Contains(t)) items.Remove(t);
    }
}
