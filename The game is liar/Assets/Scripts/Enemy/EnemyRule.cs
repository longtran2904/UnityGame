using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public interface IEnemyAction
{
    public void Execute(EnemyInstance enemy);
}

public enum RuleType
{
    Alien,
    Jellyfish,
    Maggot
}

class EnemyRule
{
    public int priority;

    public int numberOfTags
    {
        get => All.Length + Any.Length + None.Length;
    }
    private EnemyTag[] All;
    private EnemyTag[] Any;
    private EnemyTag[] None;

    public enum ChooseActionType { Repeat, Shuffle }
    private ChooseActionType actionType;
    private int currentAction;
    private IEnemyAction[] actions;

    public EnemyRule(int priority, ChooseActionType actionType, EnemyTag[] All = null, EnemyTag[] Any = null, EnemyTag[] None = null, IEnemyAction[] actions = null)
    {
        this.priority   = priority;
        this.All        = All;
        this.Any        = Any;
        this.None       = None;
        this.actions    = actions;
        this.actionType = actionType;
    }

    public bool CheckTags(EnemyInstance enemy)
    {
        bool all = true, any = true, none = true;

        // 'All' tags check
        foreach (var tag in All)
        {
            if (enemy.tags.Contains(tag))
            {
                all = false;
                break;
            }
        }

        // 'Any' tags check
        if (Any.Length > 0) any = false;
        foreach (var tag in Any)
        {
            if (enemy.tags.Contains(tag))
            {
                any = true;
                break;
            }
        }

        // 'None' tags check
        foreach (var tag in None)
        {
            if (enemy.tags.Contains(tag))
                none = false;
                break;
        }

        return all && any && none;
    }

    public IEnemyAction GetAction()
    {
        int prevAction = currentAction;
        int nextAction = currentAction + 1;
        currentAction = nextAction < actions.Length - 1 ? nextAction : 0;
        if (actionType == ChooseActionType.Shuffle && nextAction > actions.Length - 1)
            MathUtils.Shuffle(actions);
        return actions[prevAction];
    }
}

class EnemyManager : MonoBehaviour
{
    EnemyInstance[] enemies;
    Dictionary<RuleType, EnemyRule[]> ruleTable;

    private void Update()
    {
        foreach (var enemy in enemies)
        {
            UpdateTags(enemy);

            List<EnemyRule> passRules = new List<EnemyRule>();
            foreach (var type in ruleTable.Keys)
            {
                if (enemy.type == type)
                {
                    foreach (EnemyRule rule in ruleTable[type])
                    {
                        if (rule.CheckTags(enemy))
                            passRules.Add(rule);
                    }
                }
            }
            passRules.OrderBy(x => x.priority).ThenBy(x => x.numberOfTags).ElementAt(0).GetAction().Execute(enemy);
        }
    }

    void UpdateTags(EnemyInstance enemy)
    {
        if (enemy.hasWeapon)
            enemy.AddTag(EnemyTag.HasWeapon);
        if (enemy.GroundCheck())
            enemy.AddTag(EnemyTag.OnGround);
        if (enemy.CliffCheck())
            enemy.AddTag(EnemyTag.HitCliff);
        if (enemy.PlayerCheck(20))
            enemy.AddTag(EnemyTag.LineOfSight);
        if (enemy.WallCheck(.5f))
            enemy.AddTag(EnemyTag.HitWall);
        if (enemy.IsInRange(enemy.range))
            enemy.AddTag(EnemyTag.InRange);
    }
}

public enum EnemyTag
{
    HitWall,
    HitCliff,
    OnGround,
    LineOfSight,
    HasWeapon,
    WeaponReady,
    InRange
}

public class EnemyInstance : MonoBehaviour
{
    public static int numberOfEnemiesAlive = 0;

    public IntReference health;
    public IntReference damage;
    public FloatReference speed;
    public FloatReference range;

    public RangedFloat moneyDrop;
    public Material matWhite;

    public GameObject explosionParitcle;

    public bool lookAtPlayer;
    public bool damageWhenCollide;

    // This is only for showing in the inspector
#if UNITY_EDITOR
    [Header("Enemy's Ability")]
    public bool canDash;
    public bool canFlash;
    public bool curveMove;
    public bool canSplit;
    public bool canExplode;
    public bool hasWeapon;
#endif

    [Header("Enemy's Value")]
    [ShowWhen("canDash")] public float dashSpeed;

    [ShowWhen("canFlash")] public Material triggerMat;
    [ShowWhen("canFlash")] public float flashTime;
    [ShowWhen("canFlash")] public float timeBtwFlashes;

    [ShowWhen("canSplit")] public int numberOfSplits = 2;
    [ShowWhen("canSplit")] public Enemy splitEnemy;

    [ShowWhen("canExplode")] public bool explodeWhenDie;
    [ShowWhen("canExplode")] public bool explodeWhenInRange;
    [ShowWhen("canExplode")] public GameObject explodeParticle;
    [ShowWhen("canExplode")] public float explodeRange;
    [ShowWhen("explodeWhenInRange")] public float explodeTime;
    [ShowWhen("canExplode")] public string explodeSound;

    [ShowWhen("hasWeapon")] public Weapon weapon;
    private float timeBtwShots;

    public Rigidbody2D rb;
    public Player player;
    public Animator anim;
    public SpriteRenderer sr;
    public Material defMat;

    public List<EnemyTag> tags = new List<EnemyTag>();
    public List<EnemyTag> tagsToCheck = new List<EnemyTag>();
    public RuleType type;

    public void UpdateTags()
    {
        tags.Clear();
        foreach (var tag in tagsToCheck)
        {
            if (HandleTag(tag))
            {
                tags.Add(tag);
            }
        }
    }

    bool HandleTag(EnemyTag tag)
    {
        switch (tag)
        {
            case EnemyTag.HitWall:
                return WallCheck(1);
            case EnemyTag.HitCliff:
                return CliffCheck();
            case EnemyTag.OnGround:
                return GroundCheck();
            case EnemyTag.LineOfSight:
                return PlayerCheck(10);
            case EnemyTag.HasWeapon:
                return hasWeapon;
            case EnemyTag.WeaponReady:
                break;
            case EnemyTag.InRange:
                return IsInRange(range);
        }
        return false;
    }

    public void AddTag(EnemyTag tag)
    {
        if (!tags.Contains(tag))
            tags.Add(tag);
    }

    public bool GroundCheck()
    {
        Vector2 pos = (Vector2)transform.position - new Vector2(0, sr.bounds.extents.y * transform.up.y);
        Vector2 size = new Vector2(sr.bounds.size.x, 0.1f);
        ExtDebug.DrawBox(pos, size / 2, Quaternion.identity, Color.green);
        return Physics2D.BoxCast(pos, size, 0, -transform.up, 0, LayerMask.GetMask("Ground"));
    }

    /// <summary>
    ///  return true when detect a cliff
    /// </summary>
    public bool CliffCheck()
    {
        Vector2 pos = (Vector2)transform.position + new Vector2((sr.bounds.extents.x + .1f) * Mathf.Sign(rb.velocity.x), -sr.bounds.extents.y);
        InternalDebug.DrawRay(pos, Vector2.down, Color.cyan);
        return !Physics2D.Raycast(pos, Vector2.down, .1f, LayerMask.GetMask("Ground"));
    }

    public bool WallCheck(float range)
    {
        Vector2 pos = transform.position + new Vector3(sr.bounds.extents.x * transform.right.x, 0, 0);
        InternalDebug.DrawRay(pos, transform.right * range, Color.yellow);
        return Physics2D.Raycast(pos, transform.right, range, LayerMask.GetMask("Ground"));
    }

    public bool PlayerCheck(float range)
    {
        Vector2 pos = transform.position + new Vector3(sr.bounds.extents.x * transform.right.x, 0, 0);
        InternalDebug.DrawRay(pos, transform.right * range, Color.blue);
        return Physics2D.Raycast(pos, transform.right, range, LayerMask.GetMask("Player"));
    }

    public bool IsInRange(float range)
    {
        return (player.transform.position - transform.position).sqrMagnitude < range * range;
    }
}
