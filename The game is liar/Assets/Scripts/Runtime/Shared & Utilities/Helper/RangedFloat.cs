using UnityEngine;

[System.Serializable]
public struct RangedFloat
{
    public float min;
    public float max;

    public RangedFloat(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public float randomValue
    {
        get
        {
            if (min == max)
                return min;
            return Random.Range(min, max);
        }
    }
}

[System.AttributeUsage(System.AttributeTargets.Field)]
public class MinMaxAttribute : PropertyAttribute
{
    public float min;
    public float max;

    public MinMaxAttribute(float aMin, float aMax)
    {
        min = aMin;
        max = aMax;
    }

    public bool IsInRange(float aValue)
    {
        return aValue >= min && aValue <= max;
    }
}