using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/Destroy")]
public class DestroyObject : ScriptableObject
{
    public float delay;

    public void Destroy(GameObject obj)
    {
        Destroy(obj, delay);
    }
}
