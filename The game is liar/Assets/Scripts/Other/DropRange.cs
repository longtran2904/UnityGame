using UnityEngine;

[System.Serializable]
public struct DropRange
{
    public float probability;
    public int min;
    public int max;

    public DropRange(int min, float probability, int max)
    {
        this.min = min;
        this.probability = probability;
        this.max = max;
    }

    public int GetRandom()
    {
        if (Random.value <= probability) return Random.Range(min, max);
        return 0;
    }
}
