using UnityEngine;
using System;

// Prototyping
#if false
public enum AbilityType
{
    TeleportAndExplode,


    AbilityCount
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
    [MinMax(0, 10)] public RangedFloat distanceToPlayer;
    public Optional<float> distanceToTeleportX;
    public Optional<float> distanceToTeleportY;
}

[Serializable]
public struct FlashAbility
{
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
public struct ExplodeAbility
{
    public ActivationType activationType;
    public FlashAbility flashData;
    public float explodeTime;
    public float explodeRange;
    [ShowWhen("activationType", ActivationType.InRange)] public float distanceToExplode;
    public int explodeDamage;
    public GameObject explodeParticle;
    public string explodeSound;
}

[Serializable]
public struct SplitAbility
{
    public GameObject splitEnemy;
}

[Serializable]
public enum BulletPattern
{
    Gun,
    Burst,
}

[Serializable]
public struct ShootAbility
{
    public BulletPattern shootPattern;
    public float cooldown;

    [ShowWhen("shootPattern", BulletPattern.Gun)] public GunData gunData;
    [ShowWhen("shootPattern", BulletPattern.Burst)] public BurstData burstData;
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
public struct BurstData
{
    [Header("Projectile Data")]
    public int damage;
    public string projectile;
    public string sfx;

    [Header("Burst Data")]
    public int numberOfBullets;
    public int radius;
    public int waves;
    public float timeBtwWaves;

    [HideInInspector] public Vector3[] positions;
    [HideInInspector] public Quaternion[] rotations;
}
#endif
