using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/ChangeColor")]
public class ChangeColor : ScriptableObject
{
    public Color[] colors;
    public bool autoIncrement;
    [ShowWhen("autoIncrement")] public bool repeat;
    private int current = 0;

    private void OnEnable()
    {
        current = 0;
    }

    public void Change(SpriteRenderer sr)
    {
        sr.material.color = colors[current];
        if (autoIncrement)
        {
            current++;
            if (repeat) current = (int)Mathf.Repeat(current, colors.Length - 1);
            else current = Mathf.Clamp(current, 0, colors.Length - 1);
        }
    }
}
