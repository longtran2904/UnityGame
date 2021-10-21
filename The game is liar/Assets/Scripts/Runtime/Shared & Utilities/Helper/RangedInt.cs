using UnityEngine;

[System.Serializable]
public struct RangedInt
{
    [Tooltip("Min Inclusive")]
    public int min;
    [Tooltip("Max Inclusive")]
    public int max;

    public RangedInt(int min, int max)
    {
        this.min = min;
        this.max = max;
    }

    public int randomValue
    {
        get
        {
            return Random.Range(min, max + 1);
        }
    }
}
