using UnityEngine;
using System;

// NOTE: These abilities happen in an amount of time (through multiple frames) and need to be kept track of.
public enum MultipleFramesAbility
{
    None,
    JumpAttack,
    ChargeAttack,
    Shoot,
    Explode,
}

public enum ActivationType
{
    InRange,
    Die,
}

[Serializable]
public struct TeleportAbility
{
    public bool flipVertically;
    [HideInInspector] public TrailRenderer trail;
    public float distanceToPlayer;
    public Optional<float> DistanceToTeleportX;
    public Optional<float> DistanceToTeleportY;
}

[Serializable]
public struct FlashAbility
{
    public float flashTime;
    public float timeBtwFlashes;
    public Color color;
    public Material triggerMat;
}

[Serializable]
public struct ChargeAttack
{
    public int damage;
    public float dashSpeed;
    public float dashTime;
    public float chargeTime;
    public float distanceToCharge;
    public FlashAbility flashData;
}

[Serializable]
public struct JumpAttack
{
    public int jumpDamage;
    public float distanceToJump;
    public JumpData jumpData;
}

[Serializable]
public struct ExplodeAbility
{
    public ActivationType activationType;
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
    public GameObject splitEnemy;
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

    public ShootPatternData patternData;
    public GunData gunData;
}

[Serializable]
public struct GunData
{
    public int damage;
    public int numberOfBulletsEachTurn;
    public int numberOfShootTurn;
    public float fireRate;
    public float timeBtwTurn;
    public string sfx;
    public string projectile;
    public Weapon weapon;
}

[Serializable]
public struct ShootPatternData
{
    public int numberOfBullets;
    public float delayBulletTime;

    public bool rotate;
    [ShowWhen("rotate")] public float rotateSpeed;
    [ShowWhen("rotate")] public bool clockwise;

    public BulletHolder bulletHolder;
    [HideInInspector] public Projectile[] bullets;
    [HideInInspector] public Vector2[] bulletsPos;

    // Circle
    public int radius;

    // Square
    public Vector2 size;

    // Triangle
    public float distanceToCenter;
    public float rotation;
}
