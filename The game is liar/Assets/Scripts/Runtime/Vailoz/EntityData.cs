using UnityEngine;

public enum TransformType
{
    None,
    Acceleration,
    SecondOrder,
}

public enum RotateType
{
    FlipX,
    FlipY,
    RotateZ,
    LookAt,
}

public enum TransformFlag
{
    FreezeX,
    FreezeY,
    ReverseX,
    ReverseY,
}

public class EntityTransform
{
    public Entity entity;
    public Transform transform => entity.transform;
    public Vector2 pos, vel;
}

public struct MoveStat
{
    public float minSpeed, maxSpeed, acc, dec;
    public MoveStat(float speed)
    {
        minSpeed = maxSpeed = speed;
        acc = dec = 0;
    }
    public MoveStat(float acceleration, float startSpeed = 0)
    {
        minSpeed = maxSpeed = startSpeed;
        acc = dec = acceleration;
    }
}

public enum TargetOffsetType
{
    None,
    Input,
    Mouse,
    PlayerDir,
    PlayerUp,
    EntityRight,
}

public struct OffsetData
{
    public TargetOffsetType type;
    public Vector2 offset;
    
    public Vector2 GetOffset(Vector2 entityDir)
    {
        Vector2 currentDir = Vector2.one;
        Transform player = GameManager.player.transform;
        switch (type)
        {
            case TargetOffsetType.Input:       currentDir = GameInput.GetAxis();     break;
            case TargetOffsetType.Mouse:       currentDir = GameInput.GetMouseDir(); break;
            case TargetOffsetType.PlayerDir:   currentDir = player.Direction();      break;
            case TargetOffsetType.PlayerUp:    currentDir = player.up;               break;
            case TargetOffsetType.EntityRight: currentDir = entityDir;               break;
        }
        return currentDir * offset;
    }
}

public class TransformData
{
    public TransformData next;
    public Property<TransformFlag> flags;
    
    public Vector2 targetPos;
    public OffsetData targetOffset;
    
    public TransformType type;
    public RotateType rotateType;
    public float zOffset;
    
    public MoveStat stat;
    
    public static void TransformEntity(TransformData data, EntityTransform transform, float dt)
    {
        for (; data != null; data = data.next)
        {
            Vector2 targetPos = data.targetPos, entityPos = transform.pos, entityVel = transform.vel, entityDir = transform.transform.right;
            
            targetPos += data.targetOffset.GetOffset(entityDir);
            Vector2 targetDir = targetPos - entityPos;
            
            bool freezeX = data.flags.HasProperty(TransformFlag.FreezeX), reverseX = data.flags.HasProperty(TransformFlag.ReverseX);
            bool freezeY = data.flags.HasProperty(TransformFlag.FreezeY), reverseY = data.flags.HasProperty(TransformFlag.ReverseY);
            
            if (freezeX) targetDir.x = 0;
            if (reverseX)
            {
                if (freezeX) entityPos.x = targetPos.x;
                else         targetDir.x *= -1;
            }
            
            if (freezeY) targetDir.y = 0;
            if (reverseY)
            {
                if (freezeY) entityPos.y = targetPos.y;
                else         targetDir.y *= -1;
            }
            
            targetPos = entityPos + targetDir;
            targetDir.Normalize();
            
            switch (data.type)
            {
                case TransformType.Acceleration:
                {
                    Debug.Assert(data.stat.maxSpeed >= data.stat.minSpeed);
                    
                    float mag = Mathf.Max(entityVel.magnitude, data.stat.minSpeed);
                    Vector2 targetVel = targetDir * (data.stat.maxSpeed > 0 ? data.stat.maxSpeed: mag);
                    float targetMag = targetVel.magnitude; // NOTE(long): targetMag != data.stat.maxSpeed when targetDir == Vector2.zero
                    
                    entityVel = Vector2.MoveTowards(entityVel.normalized * mag, targetVel, dt * (targetMag < mag ? data.stat.dec : data.stat.acc));
                    entityPos += entityVel * dt;
                } break;
                
                case TransformType.SecondOrder:
                {
                    float k1 = 0, k2 = 0, k3 = 0;
                    entityPos.x = MathUtils.SecondOrder(dt, k1, k2, k3, targetPos.x, data.targetPos.x, entityPos.x, ref entityVel.x);
                    entityPos.y = MathUtils.SecondOrder(dt, k1, k2, k3, targetPos.y, data.targetPos.y, entityPos.y, ref entityVel.y);
                } break;
            }
            
            targetDir = targetPos - entityPos;
            Vector2 rotateDir = Quaternion.AngleAxis(data.zOffset, Vector3.forward) * targetDir;
            Vector3 rotation = Vector3.zero;
            
            switch (data.rotateType)
            {
                case RotateType.FlipX: if (rotateDir.x != 0 && Mathf.Sign(rotateDir.x) != Mathf.Sign(entityDir.x)) rotation.y = 180; break;
                case RotateType.FlipY: if (rotateDir.y != 0 && Mathf.Sign(rotateDir.y) != Mathf.Sign(entityDir.y)) rotation.x = 180; break;
                case RotateType.RotateZ: rotation.z = MathUtils.GetRotZ(rotateDir); break;
                
                case RotateType.LookAt:
                {
                    Vector3 targetForward = GameManager.player.transform.forward; // TODO(long): Abstract the player's forward vector out
                    transform.transform.rotation = MathUtils.LookRotation(rotateDir, targetForward);
                } break;
            }
            
            data.targetPos = targetPos;
            transform.pos = entityPos;
            transform.vel = entityVel;
            transform.transform.Rotate(rotation);
        }
        transform.transform.position = transform.pos;
    }
}

public enum TriggerType
{
    None,
    Health,
    Ammo,
    Distance,
    DistanceX,
    DistanceY,
    OnGround,
    InputX,
    InputY,
    ValidPos,
}

public enum TriggerFlag
{
    Inverse,
    TargetPlayer,
    DelayTrigger,
}

public class EntityTrigger
{
    public Property<TriggerFlag> flags;
    public EntityTrigger next;
    public TriggerType type;
    public InputType input;
    
    [MinMax(0, 100)]
    public RangedFloat triggerValue;
    public float bufferTime;
    
    private float timer;
    
    public EntityTrigger(TriggerType trigger, float value = 0, bool targetPlayer = false)
    {
        type = trigger;
        triggerValue = new RangedFloat(value);
        flags.SetProperty(TriggerFlag.TargetPlayer, targetPlayer);
    }
    
    public bool IsTrigger(Entity entity, EntityAction action, bool hasValidPos)
    {
        bool result = true;
        bool targetPlayer = flags.HasProperty(TriggerFlag.TargetPlayer);
        
        Entity target = targetPlayer ? GameManager.player : entity;
        Transform transform = target.transform;
        Vector2 extents = target.spriteExtents;
        
        Vector2 playerPos = (Vector2)GameManager.player.transform.position;
        Vector2 distance = (targetPlayer ? playerPos : action.transform.targetPos) - (Vector2)entity.transform.position;
        
        switch (type)
        {
            case TriggerType.Health:    result = triggerValue.InRange(target.health);                                            break;
            case TriggerType.Ammo:      result = triggerValue.InRange(target.ammo);                                              break;
            case TriggerType.Distance:  result = triggerValue.InRange(distance.magnitude);                                       break;
            case TriggerType.DistanceX: result = triggerValue.InRange(distance.x);                                               break;
            case TriggerType.DistanceY: result = triggerValue.InRange(distance.y);                                               break;
            case TriggerType.OnGround:  result = GameUtils.GroundCheck(transform.position, extents, -transform.up.y, Color.red); break;
            case TriggerType.InputX:    result = GameInput.GetAxis(AxisType.Horizontal) != 0;                                    break;
            case TriggerType.InputY:    result = GameInput.GetAxis(AxisType.Vertical  ) != 0;                                    break;
            case TriggerType.ValidPos:  result = hasValidPos;                                                                    break;
        }
        
        result &= input != InputType.None ? GameInput.GetInput(input) : true;
        if (flags.HasProperty(TriggerFlag.Inverse))
            result = !result;
        
        return result;
    }
}

public enum MoveRegionType
{
    None,
    WalkRegion,
    GroundPos,
    Manual,
}

public enum MoveTarget
{
    None,
    Input,
    Player,
    Entity,
    Region,
}

public class ActionList
{
    public ActionList next;
    public EntityAction first;
    public EntityAction current;
    
    public static void Execute(ActionList firstList, EntityTransform transform, float dt)
    {
        Entity entity = transform.entity;
        
        for (ActionList list = firstList; list != null; list = list.next)
        {
            Debug.Assert(list.first != null);
            if (list.current == null)
                list.current = list.first;
            EntityAction action = list.current;
            
            if (action.timer > action.duration)
            {
                list.current = action.next;
                action.timer = 0;
                break;
            }
            
            TransformData data = action.transform;
            
            (bool valid, Vector2 targetPos) = action.GetTargetPos(transform.pos, data.targetPos, entity.spriteExtents);
            
            if (action.timer <= action.interuptTime)
            {
                if (!action.CanTrigger(entity, valid))
                {
                    list.current = list.first;
                    action.timer = 0;
                    break;
                }
            }
            
            TransformData.TransformEntity(data, transform, dt);
            transform.pos = action.UpdateMoveRegion(action.regionType, transform.pos, entity.spriteExtents, -transform.transform.up.y);
            
            action.timer += dt;
        }
    }
}

public class EntityAction
{
    public const float MAX_DURATION_TIME = 365*24*60*60;
    
    public EntityAction next;
    public EntityTrigger trigger;
    
    public float duration;
    public float interuptTime;
    
    public MoveTarget targetType;
    public MoveRegionType regionType;
    public GameManager.SearchParameter searchParam;
    
    public TransformData transform;
    
    private Rect region;
    public float timer;
    
    public void PushTriggers(params EntityTrigger[] triggers)
    {
        for (int i = 0; i < triggers.Length - 1; ++i)
            triggers[i].next = triggers[i+1].next;
        trigger = triggers[0];
    }
    
    public bool CanTrigger(Entity entity, bool hasValidPos)
    {
        for (EntityTrigger trigger = this.trigger; trigger != null; trigger = trigger.next)
            if (!trigger.IsTrigger(entity, this, hasValidPos))
            return false;
        return true;
    }
    
    public void PingPong(float distance, bool horizontal)
    {
        targetType = MoveTarget.Region;
        regionType = MoveRegionType.GroundPos;
        region.size = (horizontal ? Vector2.right : Vector2.up) * distance;
    }
    
    public (bool, Vector2) GetTargetPos(Vector2 currentPos, Vector2 oldTarget, Vector2 spriteExtents)
    {
        bool success = true;
        Vector2 playerPos = GameManager.player.transform.position;
        Vector2 result = currentPos;
        
        switch (targetType)
        {
            case MoveTarget.Input:  result += GameInput.GetAxis(); break;
            
            case MoveTarget.Player: result = playerPos; goto case MoveTarget.Entity;
            case MoveTarget.Entity:
            {
                float upDir = Mathf.Sign(spriteExtents.y);
                spriteExtents.y = Mathf.Abs(spriteExtents.y);
                (success, result) = GameManager.SearchValidPos(result, spriteExtents, upDir, searchParam);
            } break;
            
            case MoveTarget.Region:
            {
                if (oldTarget != region.min && oldTarget != region.max)
                    result = GetRectPos(region, searchParam.filterResult);
                if (MathUtils.IsApproximate(currentPos, oldTarget))
                    result = oldTarget == region.max ? region.min : region.max;
                
                Vector2 GetRectPos(Rect rect, PositionType type)
                {
                    switch (type)
                    {
                        case PositionType.Random: return MathUtils.RandomPoint(rect.min, rect.max);
                        case PositionType.Min:    return rect.min;
                        case PositionType.Middle: return rect.center;
                        case PositionType.Max:    return rect.max;
                    }
                    Debug.Assert(false);
                    success = false;
                    return currentPos;
                }
            } break;
        }
        
        return (success, result);
    }
    
    public Vector2 UpdateMoveRegion(MoveRegionType type, Vector2 position, Vector2 extents, float dirY)
    {
        switch (type)
        {
            case MoveRegionType.WalkRegion: region = GameManager.CalculateMoveRegion(position, extents, dirY); break;
            case MoveRegionType.Manual:     region = new Rect(position, extents * 2);                          break;
            
            case MoveRegionType.GroundPos:
            {
                if (GameManager.GetGroundPos(position, extents, dirY, out _, out Vector3Int groundPos))
                    region.position = new Vector2(position.x, groundPos.y + extents.y);
            } break;
        }
        
        return (type == MoveRegionType.None && region == Rect.zero) ? position : MathUtils.Clamp(position, region.min, region.max);
    }
}

#if false
EntityAction CreatePlayerAction(Entity player, MoveStat moveStat, MoveStat gravityStat)
{
    EntityAction result = new EntityAction
    {
        trigger = new EntityTrigger(TriggerType.InputX),
        target = new TargetData(MoveTarget.Input),
        
        transform = new TransformData
        {
            type = TransformType.Acceleration,
            flags = new Property<TransformFlag>(TransformFlag.FreezeY),
            rotateType = RotateType.FlipY,
            stat = moveStat,
        },
    };
    
    result.next = new EntityAction
    {
        trigger = new EntityTrigger
        {
            flags = new Property(TriggerFlag.Inverse),
            type = TriggerType.OnGround,
            bufferTime = ,
        }
        
        transform = new TransformData
        {
            type = TransformType.Acceleration,
            targetOffset = new OffsetData { offset = Vector2.down },
            stat = gravityStat,
        },
    };
    
    return result;
}

EntityAction CreateWeaponAction(Entity weapon, Vector2 posOffset, MoveStat moveStat)
{
    return new EntityAction
    {
        target = new TargetData(MoveTarget.Player),
        transform = new TransformData
        {
            flags = new Property<TransformFlag>(TransformFlag.FreezeX, TransformFlag.ReverseX),
            targetOffset = new OffsetData { type = TargetOffsetType.PlayerDir, offset = posOffset },
            type = TransformType.SecondOrder,
            rotateType = RotateType.LookAt,
            stat = moveStat,
        },
    };
}

void PushEmptyAction(Entity entity, EntityTrigger trigger, float chargeTime, float interuptTime, float cooldownTime)
{
    return new EntityAction
    {
        trigger = trigger,
        duration = chargeTime,
        interuptTime = interuptTime,
        cooldownTime = cooldownTime,
    };
}

EntityAction MoveToPlayer(Entity entity, EntityTrigger trigger, float speed, MoveRegionType region = MoveRegionType.None)
{
    return new EntityAction
    {
        trigger = trigger,
        target = new TargetData(MoveTarget.Player, region),
        transform = new TransformData
        {
            type = TransformType.Acceleration,
            stat = new MoveStat(speed),
        }
    };
}

void Jump(Entity entity)
{
    EntityAction gravity = GetGravityAction(entity);
    gravity.transform.targetOffset.offset.y *= -1;
}

void Knockback(Entity entity, float force)
{
    EntityAction knockback = new EntityAction { transform = new TransformData() };
    knockback.transform.targetOffset = new OffsetData { type = TargetOffsetType.EntityRight, offset = Vector2.one * force };
    PushNewAction(entity, knockback);
    
    // or
    
    EntityAction action = GetMoveAction(entity);
    action.transform.next = new TransformData();
    action.transform.next.targetOffset = new OffsetData { type = TargetOffsetType.EntityRight, offset = Vector2.one * force };
}
#endif
