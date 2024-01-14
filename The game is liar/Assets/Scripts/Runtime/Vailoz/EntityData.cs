using UnityEngine;

#if NEW_AI
enum JumpState
{
    OnGround,
    InAir,
    Land,
    Jump,
}

enum AbilityState
{
    None,
    Charge,
    Execute,
    Cooldown,
}

public struct AbilityTime
{
    public float interupt;
    public float charge;
    public float execute;
    public float cooldown;
}

public struct EntityStat
{
    public int damage;
    public int health;
    
    public float range;
    public float speed;
    // acc and dec, min and max (terminal) speed
}

public class EntityAI
{
    struct AbilityData
    {
        public enum AbilityFlag
        {
            ExecuteWhenLowHealth,
            ExecuteWhenInRangeX,
            ExecuteWhenInRangeY,
        }
        
        public Property<AbilityFlag> flags;
        public int healthToExecute;
        public Vector2 distanceToExecute;
        
        public AbilityTime time;
        public EntityStat stat;
        
        public bool IsTrigger(Entity entity)
        {
            Property<AbilityFlag> flags = this.flags;
            Vector2 targetPos = GameManager.player.transform.position;
            Vector2 entityPos = entity.transform.position;
            
            bool lowHealth   = Check(AbilityFlag.ExecuteWhenLowHealth, entity.health < healthToExecute);
            bool isInRangeX  = Check(AbilityFlag.ExecuteWhenInRangeX, Mathf.Abs(targetPos.x - entityPos.x) <= distanceToExecute.x);
            bool isInRangeY  = Check(AbilityFlag.ExecuteWhenInRangeY, Mathf.Abs(targetPos.y - entityPos.y) <= distanceToExecute.y);
            
            bool Check(AbilityFlag flag, bool condition) => flags.HasProperty(flag) == condition;
            
            return lowHealth && isInRangeX && isInRangeY;
        }
    }
    
    static void SetAbilityState(Entity entity, AbilityState state)
    {
        entity.abilityState = state;
        float[] times = new float[]
        {
            entity.
        }
        entity.abilityTimer = ;
    }
    
    JumpState UpdateJumpState(Entity entity, bool pressJump)
    {
        // NOTE: We're passing -transform.up.y rather than -rb.gravityScale because:
        // 1. Some objects don't have a rigidbody. It's also in my roadmap to replace the rigidbody system entirely.
        // 2. The isGrounded only equals false if and only if the down velocity isn't zero.
        bool isGrounded = GameUtils.GroundCheck(entity.transform.position, entity.spriteExtents, -entity.transform.up.y, Color.clear);
        bool wasGrounded = entity.HasProperty(EntityProperty.IsGrounded);
        entity.SetProperty(EntityProperty.IsGrounded, isGrounded);
        
        entity.groundRemember -= Time.deltaTime;
        if (isGrounded)
            entity.groundRemember = entity.groundRememberTime;
        
        entity.fallRemember -= Time.deltaTime;
        if (!isGrounded)
            entity.fallRemember = entity.fallRememberTime;
        
        entity.jumpPressedRemember -= Time.deltaTime;
        // NOTE(long): Do I care about EntityProperty.CanJump?
        if (/*entity.HasProperty(EntityProperty.CanJump) && */pressJump)
            entity.jumpPressedRemember = entity.jumpPressedRememberTime;
        
        if ( isGrounded &&  wasGrounded) return JumpState.OnGround;
        if (!isGrounded && !wasGrounded) return JumpState.InAir;
        if (isGrounded) return JumpState.Land;
        return JumpState.Jump;
    }
    
    AbilityState UpdateAbilityState(Entity entity)
    {
        float timer = entity.abilityTimer;
        if (timer > 0)
        {
            float time = entity.time.charge;
            if (timer < time)
                return AbilityState.Charge;
            
            time += entity.time.execute;
            if (timer < time)
                return AbilityState.Execute;
            
            time += entity.time.wait;
            if (timer < time)
                return AbilityState.Wait;
            
            time += entity.time.cooldown;
            if (timer > time)
                entity.abilityTimer = 0;
        }
        return AbilityState.None;
    }
    
    Vector2 MoveX(Entity entity, float dirX)
    {
        entity.velocity = new Vector2(dirX * entity.speed, entity.rb.velocity.y);
        entity.rb.velocity = entity.velocity;
        return entity.velocity;
    }
    
    Vector2 Move(Entity entity, Vector2 dir)
    {
        entity.velocity = dir * entity.speed * Time.deltaTime;
        entity.rb.velocity = entity.velocity;
        return entity.velocity;
    }
    
    void Jump(Entity entity, Vector2 velocity)
    {
        entity.jumpPressedRemember = 0;
        entity.groundRemember = 0;
        
        // NOTE(long): Increase the y position a small amount so the ground checking will make isGrounded false next frame
        entity.transform.position += new Vector3(0, 0.05f, 0);
        entity.rb.gravityScale *= -1;
    }
    
    void DefaultJumpVFX(Entity entity, JumpState state, Vector2 velocity, Vector2 prevVelocity)
    {
        switch (state)
        {
            case JumpState.Jump:
            {
                VFXManager.PlayVFX(entity.type, entity.data, VFXKind.Jump, velocity.x, true);
            } break;
            
            case JumpState.Land:
            {
                VFXManager.PlayVFX(entity.type, entity.data, VFXKind.Fall, velocity.x, false, start =>
                                   {
                                       CapsuleCollider2D cs = entity.cd as CapsuleCollider2D;
                                       if (cs)
                                           cs.direction = start ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
                                   }, () => entity.jumpPressedRemember >= 0 && entity.groundRemember >= 0);
            } break;
            
            case JumpState.InAir:
            {
                VFXManager.PlayVFX(entity.type, entity.data, VFXKind.Fall, 0, true);
            } break;
            
            case JumpState.OnGround:
            {
                float deltaVelocity = MathUtils.Sign(velocity.x - prevVelocity.x);
                if (velocity.x == -prevVelocity.x)
                    deltaVelocity *= 2;
                VFXManager.PlayVFX(entity.type, entity.data, VFXKind.Move, deltaVelocity, velocity.x != 0);
            } break;
        }
    }
    
    Entity GetPlayer() => GameManager.player;
    Vector2 GetPlayerPos() => GetPlayer().transform.position;
    Vector2 DirToPlayer(Entity entity) => (GetPlayerPos() - (Vector2)entity.transform.position).normalized;
    
    Vector2 GetRegionPos(Entity entity, PositionType initPos = PositionType.Max)
    {
        Rect region = GameManager.CalculateMoveRegion(entity.transform.position, entity.spriteExtents, -entity.transform.up.y);
        Vector2 target = GetTargetPos(entity);
        
        if (target != region.min && target != region.max)
            target = GetRectPos(region, initPos);
        bool isTargetingMax = target == region.max;
        if (MathUtils.IsApproximate(entity.transform.position, target, .1f, isTargetingMax ? 1 : -1))
            target = isTargetingMax ? region.min : region.max;
        
        return target;
        
        Vector2 GetRectPos(Rect rect, PositionType type)
        {
            switch (type)
            {
                case PositionType.Random: return MathUtils.RandomPoint(rect.min, rect.max);
                case PositionType.Min:    return rect.min;
                case PositionType.Middle: return rect.center;
                case PositionType.Max:    return rect.max;
                default: throw new System.ComponentModel.InvalidEnumArgumentException();
            }
        }
    }
    
    void Teleport(Entity entity, float range)
    {
        Transform transform = entity.transform;
        float playerUp = GameManager.player.transform.up.y;
        
        bool canTp = false;
        Vector3 destination = GameManager.player.transform.position;
        {
            destination.y += (entity.spriteExtents.y - GameManager.player.spriteExtents.y) * playerUp;
            
            // TODO(long): Maybe teleport opposite to where the player is heading or teleport to nearby platform?
            float distance = range * Mathf.Sign(GameManager.player.transform.position.x - transform.position.x);
            canTp = IsPosValid(distance) || IsPosValid(-distance);
            
            bool IsPosValid(float offsetX)
            {
                Vector2 pos = destination + new Vector3(offsetX, 0);
                bool onGround = GameUtils.BoxCast(pos - new Vector2(0, entity.spriteExtents.y * playerUp),
                                                  new Vector2(entity.spriteExtents.x, .1f), Color.yellow);
                bool insideWall = GameUtils.BoxCast(pos + new Vector2(0, .1f * playerUp), entity.data.sr.bounds.size, Color.green);
                
                if (onGround && !insideWall)
                {
                    destination.x += offsetX;
                    return true;
                }
                return false;
            }
        }
        
        if (canTp)
        {
            if (playerUp != transform.up.y)
                transform.Rotate(180, 0, 0);
            transform.position = destination;
        }
        
        // TODO(long): PlayVFX
    }
    
    void Explode(Entity entity, float range, int damage)
    {
        // NOTE(long): If the enemy die then it will be handled by the vfx system
        if (entity.IsInRange(range, GetPlayerPos()))
            GameManager.player.Hurt(damage);
    }
    
    void Shoot(Entity entity, AbilityData shoot)
    {
        // TODO(long): Calculate critical hit and spray pattern
        bool isCritical = false;
        float rotZ = 0;
        int damage = shoot.stat.damage;
        
        Transform transform = entity.transform;
        Vector3 rotation = transform.eulerAngles + Vector3.forward * rotZ;
        Entity bullet = ObjectPooler.Spawn<Entity>(PoolType.Bullet_Normal, transform.GetChild(0).position, rotation);
        bullet.InitBullet(damage, isCritical, false);
        
        // TODO(long): Play VFX
        SetAbilityState(entity, AbilityState.Cooldown);
    }
    
    void Reload(Entity entity, WeaponStat stat)
    {
        // TODO set the update state flag
        entity.StartCoroutine(Reloading(stat, GameManager.gameUI.UpdateReload,
                                        enable =>
                                        {
                                            GameManager.gameUI.EnableReload(enable, stat.standardReload);
                                            GameInput.EnableInput(InputType.Interact, !enable);
                                            //entity.SetProperty(EntityProperty.IsReloading, enable);
                                            SetAbilityState(entity, enable ? AbilityState.Charge : AbilityState.None);
                                            entity.ammo = enable ? 0 : stat.ammo;
                                        }));
        
        System.Collections.IEnumerator Reloading(WeaponStat stat, System.Func<float, bool, bool> updateUI, System.Action<bool> enable)
        {
            enable(true);
            yield return null;
            
            float maxTime = stat.standardReload;
            float t = 0;
            bool hasReloaded = false;
            while (t <= maxTime)
            {
                yield return null;
                t += Time.deltaTime;
                if (!hasReloaded)
                {
                    bool isPerfect = updateUI(t, hasReloaded = GameInput.GetInput(InputType.Reload));
                    if (hasReloaded)
                        maxTime = isPerfect ? stat.perfectReload : stat.failedReload;
                }
            }
            
            enable(false);
        }
    }
    
    void Dash(Entity entity, AbilityData data)
    {
        SetAbilityState(entity, AbilityState.Execute, data.time);
        entity.damage = data.stat.damage;
        entity.speed  = data.stat.speed;
    }
    
    Vector2 GetTargetPos(Entity entity) => entity.targetPos;
    Vector2 UpdateTargetPos(Entity entity, Vector2 newPos)
    {
        Vector2 oldPos = entity.targetPos;
        entity.targetPos = newPos;
        return oldPos;
    }
    
    // NOTE(long): Maybe be just use entity.targetDir directly
    Vector2 GetTargetDir(Entity entity) => entity.targetPos - (Vector2)entity.transform.position;
    Vector2 UpdateTargetDir(Entity entity, Vector2 newDir)
    {
        Vector2 oldDir = GetTargetDir(entity);
        entity.targetPos = (Vector2)entity.transform.position + newDir;
        return oldDir;
    }
    
    void UpdatePlayer(Entity entity)
    {
        float moveInput = GameInput.GetAxis(AxisType.Horizontal);
        Vector2 prevVelocity = entity.velocity;
        Vector2 velocity = MoveX(entity, moveInput);
        
        if (Mathf.Sign(entity.transform.right.x) != Mathf.Sign(GameInput.GetDirToMouse(entity.transform.position).x))
            entity.transform.Rotate(0, 180, 0);
        
        JumpState state = UpdateJumpState(entity, GameInput.GetInput(InputType.Jump));
        DefaultJumpVFX(entity, state, velocity, prevVelocity);
    }
    
    void UpdateMaggot(Entity entity, AbilityData teleport, AbilityData explode)
    {
        Transform transform = entity.transform;
        Vector2 playerPos = GetPlayerPos();
        
        AbilityState state = UpdateAbilityState(entity);
        
        if (state == AbilityState.None)
        {
            bool targetPlayer = false; // TODO(long): Check the entity flags
            float moveInput = Mathf.Sign((targetPlayer ? playerPos : GetRegionPos(entity)).x - transform.position.x);
            MoveX(entity, moveInput);
            
            if (teleport.IsTrigger(entity))
            {
                Teleport(entity, teleport.stat.range);
                SetAbilityState(entity, AbilityState.Cooldown);
            }
            
            if (MathUtils.InRange(transform.position, playerPos, explode.distanceToExecute.x))
                SetAbilityState(entity, AbilityState.Charge);
        }
        else if (state == AbilityState.Execute)
            Explode(entity, explode.stat.range, explode.stat.damage);
    }
    
    void UpdateWeapon(Entity entity, Vector2 posOffset, Vector3 k, AbilityData shoot)
    {
        Entity playerEntity = GetPlayer();
        Transform player = playerEntity.transform;
        Transform transform = entity.transform;
        
        Vector2 targetPos = (Vector2)player.position + posOffset * player.Direction();
        Vector2 oldPos = UpdateTargetPos(entity, targetPos);
        float newY = MathUtils.SecondOrder(Time.deltaTime, k.x, k.y, k.z, targetPos.y, oldPos.y,
                                           transform.position.y, ref entity.velocity.y);
        transform.position.Set(targetPos.x, newY, transform.position.z);
        
        AbilityState state = UpdateAbilityState(entity);
        bool isReloading = state == AbilityState.Charge;
        
        if (!playerEntity.HasProperty(EntityProperty.IsGrounded) || isReloading)
            transform.rotation = player.rotation;
        else
            transform.rotation = MathUtils.LookRotation(GameInput.GetDirToMouse(transform.position), player.forward);
        
        if (!isReloading)
        {
            if ((entity.ammo == 0               && GameInput.GetInput(InputType.Shoot)) &&
                (entity.ammo < entity.stat.ammo && GameInput.GetInput(InputType.Reload)))
                Reload(entity, entity.stat);
            else if (state == AbilityState.Cooldown)
                UpdateAbilityState(entity);
            else if (entity.ammo > 0 && GameInput.GetInput(InputType.Shoot))
                Shoot(entity, shoot);
        }
    }
    
    void UpdateNoEye(Entity entity, AbilityData dash, AbilityData cooldown)
    {
        bool targetPlayer = false;
        Vector2 playerDir = DirToPlayer(entity);
        Move(entity, targetPlayer ? playerDir : GetTargetDir(entity));
        
        AbilityState state = UpdateAbilityState(entity);
        
        switch (state)
        {
            case AbilityState.None:
            {
                if (dash.IsTrigger(entity))
                    Dash(entity, dash);
            } break;
            
            case AbilityState.Cooldown:
            {
                if (targetPlayer)
                {
                    UpdateTargetDir(entity, playerDir * new Vector2(1, -1));
                    Dash(entity, cooldown);
                    // TODO(long): targetPlayer = false
                }
                else
                {
                    Entity prefab = ObjectPooler.GetDefaultObject<Entity>(PoolType.Enemy_NoEye);
                    entity.speed = prefab.speed;
                    entity.health = prefab.health;
                    // TODO(long): targetPlayer = true
                }
            } break;
        }
    }
}
#endif
