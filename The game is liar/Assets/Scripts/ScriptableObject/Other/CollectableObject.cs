using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Collectable")]
public class CollectableObject : ScriptableObject
{
    public Sprite icon;
    public Dialogue data;

    public void Collect(CollectableRuntimeSet storage)
    {
        storage.Add(this);
    }
}
