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
    Randomly,
    Player,
    AroundPlayer
}

[Serializable]
public struct RunData
{
    public float runSpeed;
    public bool onGround;
}

[Serializable]
public struct JumpData
{
    public float jumpForce;
    public float jumpAngle;
    public float timeBtwJumps;
    public bool onGround;
}
public enum FlyPattern
{
    Linear,
    Quadratic,
    Cubic,
    Sine,
}

[Serializable]
public struct FlyData
{
    public FlyPattern type;
    public float flySpeed;
    public Vector2[] points;
}