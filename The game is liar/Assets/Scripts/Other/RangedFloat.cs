using UnityEngine;

[System.Serializable]
public struct RangedFloat
{
    [Tooltip("Min Inclusive")]
    public float min;
    [Tooltip("Max Inclusive")]
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

    public MinMaxAttribute(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public bool IsInRange(float value)
    {
        return value >= min && value <= max;
    }
}