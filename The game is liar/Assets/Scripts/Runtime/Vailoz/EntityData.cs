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
    //public Vector2 pos { get => transform.position; set => transform.position = value; }
    public Vector2 pos;
    public Vector2 vel;
    
    public Transform transform => entity.transform;
    public Vector2 gravityDir => (Physics2D.gravity * /*rb.*/gravityScale).normalized;
    public Vector2 right => entity.transform.right;
    
    public float gravityScale;
    private Rigidbody2D rb;
    
    public EntityTransform(Entity entity)
    {
        this.entity = entity;
        pos = entity.transform.position;
        rb = entity.GetComponent<Rigidbody2D>();
        if (rb)
        {
            gravityScale = rb.gravityScale;
            rb.gravityScale = 0;
        }
    }
    
    public void Update()
    {
        if (rb)
        {
            if (rb.isKinematic)
                transform.transform.position = pos;
            else
                rb.velocity = vel;
        }
        else
            transform.transform.position = pos;
    }
    
    public void Reset()
    {
        pos = transform.transform.position;
        if (rb)
            vel = rb.velocity;
    }
}

public struct MoveStat
{
    public float minSpeed, maxSpeed, acc, dec;
    public float k1, k2, k3;
    public float drag;
    
    public MoveStat(float speed, float accel = 0)
    {
        minSpeed = maxSpeed = speed;
        acc = dec = accel;
        drag = 0;
        k1 = k2 = k3 = 0;
    }
    
    public static MoveStat SecondOrder(float f, float z, float r)
    {
        return new MoveStat
        {
            k1 = z / (Mathf.PI * f),
            k2 = 1 / ((2 * Mathf.PI * f) * (2 * Mathf.PI * f)),
            k3 = r * z / (2 * Mathf.PI * f),
        };
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
    GravityDir,
}

public struct OffsetData
{
    public TargetOffsetType type;
    public Vector2 offset;
    
    public Vector2 GetOffset(EntityTransform transform)
    {
        Vector2 currentDir = Vector2.one;
        Transform player = GameManager.player.transform;
        switch (type)
        {
            case TargetOffsetType.Input:       currentDir = GameInput.GetAxis();     break;
            case TargetOffsetType.Mouse:       currentDir = GameInput./*GetMouseDir()*/GetDirToMouse(transform.pos); break;
            case TargetOffsetType.PlayerDir:   currentDir = player.Direction();      break;
            case TargetOffsetType.PlayerUp:    currentDir = player.up;               break;
            case TargetOffsetType.EntityRight: currentDir = transform.right;         break;
            case TargetOffsetType.GravityDir:  currentDir = transform.gravityDir;    break;
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
    
    public static void TransformEntity(TransformData data, EntityTransform transform, Vector2 targetPos, float dt)
    {
        for (; data != null; data = data.next)
        {
            Vector2 entityPos = transform.pos, entityVel = transform.vel;
            Vector2 oldVel = entityVel;
            
            Vector2 targetOffset = data.targetOffset.GetOffset(transform);
            targetPos += targetOffset;
            Vector2 targetDir = targetPos - entityPos;
            
            bool freezeX = data.flags.HasProperty(TransformFlag.FreezeX), reverseX = data.flags.HasProperty(TransformFlag.ReverseX);
            bool freezeY = data.flags.HasProperty(TransformFlag.FreezeY), reverseY = data.flags.HasProperty(TransformFlag.ReverseY);
            
            if (freezeX) targetDir.x = entityVel.x = 0;
            if (reverseX)
            {
                if (freezeX) entityPos.x = targetPos.x;
                else         targetDir.x *= -1;
            }
            
            if (freezeY) targetDir.y = entityVel.y = 0;
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
                    
                    float currentMag = Mathf.Max(entityVel.magnitude, data.stat.minSpeed);
                    Vector2 currentVel = (entityVel == Vector2.zero ? targetDir : entityVel.normalized) * currentMag;
                    
                    float targetMag = data.stat.maxSpeed;
                    if (targetMag == 0)
                        targetMag = currentMag + 1000; // This can be any value so long as it's greater than acc * dt
                    if (targetDir == Vector2.zero)
                        targetMag = 0; // This is for the acc calculation below
                    Vector2 targetVel = targetDir * targetMag;
                    
                    float acc = targetMag < currentMag ? data.stat.dec : data.stat.acc;
                    
                    if (acc == 0)
                        entityVel = targetVel;
                    else
                        entityVel = Vector2.MoveTowards(currentVel, targetVel, acc * dt);
                    //https://forum.unity.com/threads/drag-factor-what-is-it.85504/#post-1827892
                    //https://www.reddit.com/r/Unity3D/comments/n13or0/a_function_for_calculating_rigidbody2d_drag_from/
                    entityVel *= (1.0f / (1.0f + dt * data.stat.drag));
                    entityPos += entityVel * dt;
                } break;
                
                case TransformType.SecondOrder:
                {
                    float k1 = data.stat.k1, k2 = data.stat.k2, k3 = data.stat.k3;
                    if (!freezeX)
                        entityPos.x = MathUtils.SecondOrder(dt, k1, k2, k3, targetPos.x, data.targetPos.x, entityPos.x, ref entityVel.x);
                    if (!freezeY)
                        entityPos.y = MathUtils.SecondOrder(dt, k1, k2, k3, targetPos.y, data.targetPos.y, entityPos.y, ref entityVel.y);
                } break;
            }
            
            targetDir = targetPos - entityPos;
            Vector2 rotateDir = Quaternion.AngleAxis(data.zOffset, Vector3.forward) * targetDir;
            Vector3 rotation = Vector3.zero;
            Vector2 entityDir = transform.transform.Direction();
            
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
            
            if (freezeX) entityVel.x = oldVel.x;
            if (freezeY) entityVel.y = oldVel.y;
            
            transform.pos = entityPos;
            transform.vel = entityVel;
            transform.transform.Rotate(rotation);
        }
        //transform.transform.position = transform.pos;
        transform.Update();
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
    
    public EntityTrigger(TriggerType trigger, float value = 0, bool targetPlayer = false, bool inverse = false)
    {
        type = trigger;
        triggerValue = new RangedFloat(value);
        flags.SetProperty(TriggerFlag.TargetPlayer, targetPlayer);
        flags.SetProperty(TriggerFlag.Inverse, inverse);
    }
    
    public EntityTrigger(InputType trigger)
    {
        input = trigger;
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
        
        if (input != InputType.None)
            result &= GameInput.GetInput(input);
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
    Entity,
}

public enum MoveTarget
{
    None,
    Input,
    Player,
    Entity,
    Region,
    Custom,
}

public class ActionList
{
    public ActionList next;
    public EntityAction first;
    public EntityAction current;
    
    public static void Execute(ActionList firstList, EntityTransform transform, float dt)
    {
        Entity entity = transform.entity;
        transform.Reset();
        
        for (ActionList list = firstList; list != null; list = list.next)
        {
            Debug.Assert(list.first != null);
            if (list.current == null)
                list.current = list.first;
            EntityAction action = list.current;
            
            if (action.timer > action.duration)
            {
                action.timer = 0;
                if (action.next != null)
                {
                    list.current = action.next;
                    continue;
                }
            }
            
            TransformData data = action.transform;
            
            (bool valid, Vector2 targetPos) = action.GetTargetPos(transform.pos, data.targetPos, entity.spriteExtents);
            
            bool onGround = GameUtils.GroundCheck(transform.transform.position, entity.spriteExtents, -transform.transform.up.y, Color.red);
            
            if (action.timer <= action.interuptTime)
            {
                if (!action.CanTrigger(entity, valid))
                {
                    list.current = list.first;
                    action.timer = 0;
                    continue;
                }
            }
            
            TransformData.TransformEntity(data, transform, targetPos, dt);
            data.targetPos = targetPos;
            transform.pos = action.UpdateMoveRegion(action.regionType, transform.pos, entity.spriteExtents, -transform.transform.up.y);
            
            action.timer += dt;
        }
    }
    
    public EntityAction PushAction(TriggerType type, MoveStat stat)
    {
        return PushAction(new EntityAction
                          {
                              trigger = new EntityTrigger(type),
                              transform = new TransformData
                              {
                                  type = TransformType.Acceleration,
                                  stat = stat,
                              },
                          });
    }
    
    public EntityAction PushAction(InputType type, MoveStat stat)
    {
        return PushAction(new EntityAction
                          {
                              trigger = new EntityTrigger(type),
                              transform = new TransformData
                              {
                                  type = TransformType.Acceleration,
                                  stat = stat,
                              },
                          });
    }
    
    public EntityAction PushAction(MoveStat stat)
    {
        return PushAction(new EntityAction
                          {
                              transform = new TransformData
                              {
                                  type = stat.k2 == 0 ? TransformType.Acceleration : TransformType.SecondOrder,
                                  stat = stat,
                              },
                          });
    }
    
    public EntityAction PushAction(EntityAction action)
    {
        if (first == null)
            first = action;
        else
        {
            ActionList nextList = new ActionList();
            nextList.next = next;
            next = nextList;
            next.first = action;
        }
        
        if (action.transform == null)
            action.transform = new TransformData();
        
        return action;
    }
    
    public EntityAction PushAction() => PushAction(new EntityAction());
    
    public static ActionList CreatePlayer(MoveStat move, MoveStat fall, bool canMove)
    {
        ActionList result = new ActionList();
        
        if (canMove)
            result.PushAction(/*TriggerType.InputX, */move).SetTarget(MoveTarget.Input).SetFreezeAxis(false, true);
        EntityAction fallAction = result.PushAction(fall).SetTargetOffset(TargetOffsetType.GravityDir, Vector2.up).SetFreezeAxis(true);
        // NOTE(long): Do we need this condition
        fallAction.PushTriggers(new EntityTrigger(TriggerType.OnGround, inverse: true));
        
        result.PushAction().SetTargetOffset(TargetOffsetType.Mouse).SetRotation(RotateType.FlipX);
        result.PushAction().SetTargetOffset(TargetOffsetType.GravityDir, Vector2.down).SetRotation(RotateType.FlipY);
        
        return result;
    }
    
    public static ActionList CreateWeapon(MoveStat move, Vector2 posOffset, float zOffset = 0)
    {
        ActionList result = new ActionList();
        
        EntityAction moveAction = result.PushAction(move);
        moveAction.SetTarget(MoveTarget.Player);
        moveAction.SetTargetOffset(TargetOffsetType.PlayerDir, posOffset);
        moveAction.PushSubAction().SetTargetOffset(TargetOffsetType.Mouse).SetRotation(RotateType.LookAt);
        //moveAction.SetRotation(RotateType.LookAt, zOffset);
        moveAction.SetStickAxis(true);
        
        return result;
    }
    
    public static ActionList CreateMaggot(MoveStat stat, Vector2 distanceToTeleport, RangedFloat teleportOffset, float waitAfterTp)
    {
        ActionList result = new ActionList();
        
        EntityAction moveAction = result.PushAction(stat);
        moveAction.TargetRegion(MoveRegionType.WalkRegion, PositionType.Max);
        
        EntityAction teleportation = moveAction.PushSubAction();
        teleportation.PushTriggers(new EntityTrigger(TriggerType.DistanceX, distanceToTeleport.x, true),
                                   new EntityTrigger(TriggerType.DistanceY, distanceToTeleport.y, true),
                                   new EntityTrigger(TriggerType.ValidPos));
        teleportation.SetTarget(MoveTarget.Player);
        teleportation.SetSearchParam(Vector2.right * teleportOffset.min, Vector2.right * teleportOffset.max, RegionType.Ground);
        teleportation.SetStickAxis(true, true);
        teleportation.PushWaitAction(waitAfterTp);
        
        return result;
    }
    
    public static ActionList CreateNoEye(MoveStat moveStat, MoveStat dashStat, MoveStat cooldownStat, float distanceToDash,
                                         float interuptTime, float chargeDuration, float dashDuration, float cooldownDuration)
    {
        ActionList result = new ActionList();
        
        EntityAction moveAction = result.PushAction(moveStat);
        moveAction.SetTarget(MoveTarget.Player);
        
        EntityAction chargeAction = moveAction.PushWaitAction(chargeDuration, interuptTime);
        chargeAction.PushTriggers(new EntityTrigger(TriggerType.Distance, distanceToDash, true));
        
        EntityAction dashAction = chargeAction.PushSubAction(dashStat, dashDuration);
        dashAction.SetTarget(MoveTarget.Player);
        
        EntityAction cooldownAction = chargeAction.PushSubAction(cooldownStat, cooldownDuration);
        dashAction.SetTarget(MoveTarget.Player);
        cooldownAction.SetTargetOffset(TargetOffsetType.None, -Vector2.one);
        
        return result;
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
    
    public EntityAction PushSubAction()
    {
        EntityAction result = new EntityAction();
        result.transform = new TransformData();
        next = result;
        return result;
    }
    
    public EntityAction PushSubAction(MoveStat stat, float duration = 0)
    {
        EntityAction result = PushSubAction();
        result.transform.stat = stat;
        result.duration = duration;
        result.transform.type = TransformType.Acceleration;
        return result;
    }
    
    public EntityAction PushWaitAction(float waitTime, float interuptTime = 0)
    {
        EntityAction waitAction = PushSubAction();
        waitAction.duration = waitTime;
        waitAction.interuptTime = interuptTime;
        return waitAction;
    }
    
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
    
    public EntityAction SetRotation(RotateType rotateType, float zOffset = 0)
    {
        transform.rotateType = rotateType;
        transform.zOffset = zOffset;
        return this;
    }
    
    public EntityAction SetTarget(MoveTarget target)
    {
        targetType = target;
        return this;
    }
    
    public EntityAction TargetRegion(MoveRegionType regionType, PositionType posType)
    {
        this.regionType = regionType;
        searchParam.filterResult = posType;
        targetType = MoveTarget.Region;
        return this;
    }
    
    public EntityAction SetSearchParam(Vector2 minSize, Vector2 maxSize, RegionType searchType, PositionType filter = PositionType.Random)
    {
        searchParam = new GameManager.SearchParameter
        {
            minSearchSize = minSize,
            maxSearchSize = maxSize,
            posType = searchType,
            filterResult = filter
        };
        return this;
    }
    
    public EntityAction SetTargetOffset(TargetOffsetType type) => SetTargetOffset(type, Vector2.one);
    public EntityAction SetTargetOffset(TargetOffsetType type, Vector2 offset)
    {
        transform.targetOffset = new OffsetData
        {
            type = type,
            offset = offset,
        };
        return this;
    }
    
    public void PingPong(float distance, bool horizontal)
    {
        targetType = MoveTarget.Region;
        regionType = MoveRegionType.GroundPos;
        region.size = (horizontal ? Vector2.right : Vector2.up) * distance;
    }
    
    public EntityAction SetStickAxis(bool stickX = false, bool stickY = false)
    {
        if (stickX) transform.flags.SetProperties(TransformFlag.FreezeX, TransformFlag.ReverseX);
        if (stickY) transform.flags.SetProperties(TransformFlag.FreezeY, TransformFlag.ReverseY);
        return this;
    }
    
    public EntityAction SetFreezeAxis(bool freezeX = false, bool freezeY = false)
    {
        if (freezeX) transform.flags.SetProperties(TransformFlag.FreezeX);
        if (freezeY) transform.flags.SetProperties(TransformFlag.FreezeY);
        return this;
    }
    
    public (bool, Vector2) GetTargetPos(Vector2 currentPos, Vector2 oldTarget, Vector2 spriteExtents)
    {
        bool success = true;
        Vector2 playerPos = GameManager.player.transform.position;
        Vector2 result = currentPos;
        
        switch (targetType)
        {
            case MoveTarget.Input:  result += GameInput.GetAxis(); break;
            
            case MoveTarget.Custom: result = oldTarget; goto case MoveTarget.Entity;
            case MoveTarget.Player: result = playerPos; goto case MoveTarget.Entity;
            case MoveTarget.Entity:
            {
                float upDir = Mathf.Sign(spriteExtents.y);
                spriteExtents.y = Mathf.Abs(spriteExtents.y);
                (success, result) = GameManager.SearchValidPos(result, spriteExtents, upDir, searchParam);
            } break;
            
            case MoveTarget.Region:
            {
                result = oldTarget;
                if (oldTarget != region.min && oldTarget != region.max)
                    result = GetRectPos(region, searchParam.filterResult);
                bool isTargetingMax = oldTarget == region.max;
                if (MathUtils.IsApproximate(currentPos, oldTarget, .1f, isTargetingMax ? 1 : -1))
                    result = isTargetingMax ? region.min : region.max;
                
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
            case MoveRegionType.Entity:     region.position = position;                                        break;
            
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

void UpdatePlayer(Entity entity)
{
    float moveInput = entity.GetMoveDir().x;
    Vector2 velocity = entity.Move(Vector2.right * moveInput);
    
    bool isGrounded = ;
    bool wasGrounded = ;
    
    JumpState state = entity.UpdateJumpState(InputType.Jump, isGrounded, wasGrounded);
    
    if (state == JumpState.Jump)
        entity.Jump(velocity);
    else
        entity.HandleJumpState(state, velocity);
    
    float facingDir = entity.GetRotation(RotateDir.X);
    if (facingDir != moveInput)
        entity.FlipRotation(RotateDir.X);
}

void UpdateMaggot(Entity entity, TriggerData tpTrigger, TriggerData expTrigger, GameManager.SearchParameter teleParam, float cooldownTime, float waitTime)
{
    MoveInput moveInput = entity.GetMoveInput();
    if (moveInput.flags.HasProperty(InputFlag.Trigger))
    {
        entity.transform.position = moveInput.target;
        entity.SetWaitState(waitTime);
        entity.SetCooldown(cooldownTime);
    }
    else
        entity.Move(Vector2.right * moveInput.GetTargetDir().x);
    
    if (tpTrigger.IsTrigger(entity))
        entity.SetSearch(teleParam);
    
    if (expTrigger.IsTrigger(entity))
        entity.Explode();
}
#endif
