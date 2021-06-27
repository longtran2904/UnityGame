using UnityEngine;
using System;

public enum EnemyAbility
{
    JumpAttack,
    DashAttack,
    Shoot,
    Explode,
    Split,
    ElementalEffect,
    Teleport,
}

public enum ActivationType
{
    None,
    InRange,
    LowHP,
    Die,
}

[Serializable]
public struct TeleportAbility
{
    public TargetType targetType;
    public TrailRenderer trail;
    public float distanceToPlayer;
    public Optional<float> DistanceToTeleportX;
    public Optional<float> DistanceToTeleportY;
}

[Serializable]
public struct FlashAbility
{
    public float flashTime;
    public float timeBtwFlashes;
    public bool stopWhileFlashing;
    public Color color;
    public Material triggerMat;
}

[Serializable]
public struct DashAttack
{
    public int dashDamage;
    public float dashSpeed;
    public float dashTime;
    public float chargeTime;
    public FlashAbility flashData;
}

[Serializable]
public struct JumpAttack
{
    public int jumpDamage;
    public float jumpForce;
    public float jumpAngle;
}

[Serializable]
public struct ExplodeAbility
{
    public FlashAbility flashData;
    public float explodeTime;
    public float explodeRange;
    public float distanceToExplode;
    public int explodeDamage;
    public GameObject explodeParticle;
    public string explodeSound;
}

[Serializable]
public struct SplitAbility
{
    public int numberOfSplits;
    public Enemy splitEnemy;
}

[Serializable]
public enum BulletPattern
{
    Gun,
    Circle,
    Square,
    Triangle,
}

[Serializable]
public struct ShootAbility
{
    public BulletPattern shootPattern;
    public Weapon weapon;

    public ShootPatternData patternData;
    public BulletData data;

    // Circle
    public int radius;

    // Square
    public Vector2 size;

    // Triangle
    public float distanceToCenter;
    public float rotation;
}

public struct BulletData
{
    public int damage;
    public float fireRate;
    public int numberOfBulletsEachTurn;
    public int numberOfShootTurn;
    public string sfx;
    public string projectile;
    public Vector3 shootPos;
}

[Serializable]
public struct ShootPatternData
{
    public int numberOfBullets;
    public float delayBulletTime;

    public bool rotate;
    [ShowWhen("rotate")] public float rotateSpeed;
    [ShowWhen("rotate")] public bool clockwise;

    public GameObject bulletHolderObj;
    public Transform bulletHolder;
    public Projectile[] bullets;
}