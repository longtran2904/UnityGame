using UnityEngine;
using System;

// Prototyping
#if false
public enum MoveType
{
    Run,
    Fly,
}

public enum TargetType
{
    Random,
    Player,
}

public enum MoveState
{
    Move,
    Wait,
}

[Serializable]
public struct RunData
{
    public TargetType target;
    public float runSpeed;
}

[Serializable]
public struct JumpData
{
    public float jumpForce;
    public float jumpAngle;
    public float timeBtwJumps;
}

public enum FlyPattern
{
    Linear,
    Curve,
}

[Serializable]
public struct FlyData
{
    public FlyPattern pattern;
    public float flySpeed;

    [ShowWhen("pattern", FlyPattern.Curve)] public RangedFloat aX;
    [ShowWhen("pattern", FlyPattern.Curve)] public RangedFloat aY;
    [ShowWhen("pattern", FlyPattern.Curve)] public RangedFloat bX;
    [ShowWhen("pattern", FlyPattern.Curve)] public RangedFloat bY;
    [ShowWhen("pattern", FlyPattern.Curve)] public RangedFloat yMutiplier;

    [HideInInspector] public Vector2 start;
    [HideInInspector] public Vector2 end;
    [HideInInspector] public Vector2 curvePoint1;
    [HideInInspector] public Vector2 curvePoint2;
}
#endif