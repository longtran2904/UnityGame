using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ChangeColor : ScriptableObject
{
    public Color color;

    public void Change(SpriteRenderer sr)
    {
        sr.material.color = color;
    }
}
