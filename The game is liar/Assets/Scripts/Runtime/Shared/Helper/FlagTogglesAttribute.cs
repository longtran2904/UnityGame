using UnityEngine;
using System;

public class FlagTogglesAttribute : PropertyAttribute
{
    public int columnCount;
    public int maxDisplayCount;

    public FlagTogglesAttribute(int columnCount = 1, int maxDisplayCount = 0)
    {
        this.columnCount = columnCount;
        this.maxDisplayCount = maxDisplayCount;
    }
}