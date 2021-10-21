using UnityEngine;
using System;

[Serializable]
public struct CustomLine : IEquatable<CustomLine>
{
    public Vector2 start;
    public Vector2 end;
    public float size;
    public float length
    {
        get
        {
            return Vector2.Distance(start, end);
        }
    }
    public Vector2 center
    {
        get
        {
            return (start - end) / 2;
        }
    }

    public CustomLine(Vector2 start, Vector2 end, int size = 0)
    {
        this.start = start;
        this.end = end;
        this.size = size;
    }

    public bool Overlaps(CustomLine other)
    {
        return Equals(other) || (start.Equals(other.end) && end.Equals(other.start));
    }

    public bool Equals(CustomLine other)
    {
        return start.Equals(other.start) && end.Equals(other.end);
    }

    public override string ToString()
    {
        return $"Start: {start}, End: {end}, Size: {size}";
    }
}
