using UnityEngine;
using System;

public enum MoveType
{
    Run,
    Jump,
    Fly
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
    Stop,
}

[Serializable]
public struct RunData
{
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
    Quadratic,
    Cubic,
}

[Serializable]
public struct FlyData
{
    public FlyPattern type;
    public float flySpeed;

    [ShowWhen("type", new object[] { FlyPattern.Quadratic, FlyPattern.Cubic })] public RangedFloat aX;
    [ShowWhen("type", new object[] { FlyPattern.Quadratic, FlyPattern.Cubic })] public RangedFloat aY;
    [ShowWhen("type", FlyPattern.Cubic)] public RangedFloat bX;
    [ShowWhen("type", FlyPattern.Cubic)] public RangedFloat bY;
    [ShowWhen("type", new object[] { FlyPattern.Quadratic, FlyPattern.Cubic })] public RangedFloat yMutiplier;

    [HideInInspector] public Vector2 start;
    [HideInInspector] public Vector2 end;
    [HideInInspector] public Vector2 curvePoint;
    [HideInInspector] public Vector2 secondCurvePoint;
}