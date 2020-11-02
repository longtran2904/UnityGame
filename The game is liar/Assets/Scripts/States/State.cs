using System.Collections;
using UnityEngine;

public enum StatusType { Burn, Bleed, Poison, Freeze, Slow, Blind, Injured }

[System.Serializable]
public class State
{
    public StatusType type;
    public float duration;
    [ShowWhen("type", new object[] { StatusType.Freeze, StatusType.Slow, StatusType.Blind, StatusType.Injured })]
    public float percent;
    [ShowWhen("type", new object[] { StatusType.Burn, StatusType.Bleed, StatusType.Poison })]
    public int damage;
    [ShowWhen("type", new object[] { StatusType.Burn, StatusType.Bleed, StatusType.Poison })]
    public float timeBtwHits;

    // For damage over time type
    public State(StatusType type, float duration, int damage, float timeBtwHits)
    {
        this.type = type;
        this.duration = duration;
        this.damage = damage;
        this.timeBtwHits = timeBtwHits;
    }

    // For normal type
    public State(StatusType type, float duration, float percent)
    {
        this.type = type;
        this.duration = duration;
        this.percent = percent;
    }
}
