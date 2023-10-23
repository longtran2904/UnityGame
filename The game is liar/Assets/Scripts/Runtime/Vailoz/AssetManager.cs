using UnityEngine;
using System.Collections;

public enum EntityType
{
    None,
    Player,
    Bullet,
    Weapon,
    Maggot,
    NoEye,
    Cell,
    DamagePopup
}

public struct VFXData
{
    public Material whiteMat;
    public Transform transform;
    public SpriteRenderer sr;
    public Animator anim;
    public TrailRenderer trail;
    public TMPro.TextMeshPro text;
    public Coroutine routine;
}

public enum VFXKind
{
    None,
    Jump,
    Fall,
    Move,
    Attack,
    Hurt,
    Life,
}

public class VFXManager
{
    public static void PlayVFX(EntityType type, VFXData data, VFXKind state, float value, bool start,
                               System.Action<bool> play = null, System.Func<bool> exit = null)
    {
        MonoBehaviour runner = data.transform.GetComponent<MonoBehaviour>();
        VFX vfx = new VFX();
        
        switch (type)
        {
            case EntityType.Player:
            {
                Transform player = data.transform;
                ParticleSystem  leftDust = player.Find( "Left Dust").GetComponent<ParticleSystem>();
                ParticleSystem rightDust = player.Find("Right Dust").GetComponent<ParticleSystem>();
                float dirX = value;
                RangedFloat timeBtwFootsteps = new RangedFloat(.25f, .3f);
                
                switch (state)
                {
                    case VFXKind.Jump:
                    {
                        vfx.SetBase(AudioType.Player_Jump, ShakeMode.PlayerJump);
                        vfx.PlayParticles(dirX >= 0 ? leftDust : null, dirX <= 0 ? rightDust : null);
                        vfx.SetOffset(new Vector2(-.25f, .25f), .25f, true);
                        vfx.PushNewVFX(.2f).FlipObject(true);
                    } break;
                    
                    case VFXKind.Fall:
                    {
                        if (!start)
                        {
                            vfx.SetBase(AudioType.Player_Land, ShakeMode.PlayerJump);
                            vfx.PlayParticles(dirX >= 0 ? leftDust : null, dirX <= 0 ? rightDust : null);
                            vfx.SetOffset(new Vector2(.25f, -.25f), .25f, true);
                            vfx.SetAnimation(dirX == 0 ? "Idle" : "Move");
                            if (dirX != 0)
                                AudioManager.RepeatAudio(runner, AudioType.Player_Footstep, timeBtwFootsteps);
                        }
                        else
                        {
                            vfx.SetAnimation("Fall");
                            AudioManager.RepeatAudio(runner, AudioType.None);
                        }
                    } break;
                    
                    case VFXKind.Move:
                    {
                        if (Mathf.Abs(dirX) == 1)
                        {
                            AudioManager.RepeatAudio(runner, start ? AudioType.Player_Footstep : AudioType.None, timeBtwFootsteps);
                            vfx.SetAnimation(start ? "Move" : "Idle");
                        }
                        
                        if (dirX != 0) vfx.PlayParticles(dirX > 0 ? leftDust : rightDust);
                    } break;
                    
                    case VFXKind.Hurt:
                    {
                        GameObject hitEffect = player.Find("PlayerHitEffect").gameObject;
                        vfx.FlashEntity(.1f, .1f, new Color(1, 1, 1, .5f));
                        vfx.PushNewVFX().SetBase(AudioType.Player_Hurt, ShakeMode.Medium).FadeEffect(.15f, .8f, VFXType.Camera);
                        vfx.nextVFX.ToggleObjects(true, hitEffect).flags.SetProperties(VFXFlag.StopTime, VFXFlag.StopAnimation);
                    } break;
                    
                    case VFXKind.Life:
                    {
                        Debug.Assert(false, "TODO: NOT IMPLEMENTED!");
                    } break;
                }
            } break;
            
            case EntityType.DamagePopup:
            {
                if (IsSpawnEffect())
                {
                    bool isCritical = value < 0;
                    int damage = (int)Mathf.Abs(value);
                    float duration = .6f;
                    float fadeTime = .45f;
                    
                    vfx.ShowText(damage.ToString(), isCritical ? 3.0f : 2.5f, isCritical ? Color.red : Color.white);
                    vfx.SetOffset(Vector2.one * .5f, duration, true, overTime: true);
                    vfx.PushNewVFX(duration).SetOffset(Vector2.one * -0.5f, duration, true, overTime: true);
                    vfx.nextVFX.PushNewVFX(.1f).FadeEffect(fadeTime, 0, VFXType.Text).flags.SetProperty(VFXFlag.OverTime, true);
                }
            } break;
            
            case EntityType.Bullet:
            {
                if (IsDeathEffect())
                    vfx.SetBase(AudioType.Weapon_Hit_Wall, poolType: PoolType.VFX_Destroyed_Bullet);
            } break;
            
            case EntityType.Cell:
            {
                if (IsDeathEffect())
                    vfx.SetBase(AudioType.Game_Pickup);
            } break;
            
            case EntityType.Weapon:
            {
                switch (state)
                {
                    case VFXKind.Attack:
                    {
                        if (start) // true is shooting, false is reloading
                        {
                            // TODO(long): Have shakeDir
                            vfx.SetBase(AudioType.Player_Shoot, ShakeMode.GunKnockback);
                            vfx.ToggleObjects(true, data.transform.Find("MuzzleFlash").gameObject);
                            vfx.duration = 0.08f;
                        }
                        else
                        {
                            Debug.Assert(false, "TODO: NOTE IMPLEMENTED!");
                        }
                    } break;
                }
            } break;
            
            case EntityType.Maggot:
            {
                switch (state)
                {
                    case VFXKind.Move:
                    {
                        vfx.EmitTrail(.2f, .1f).PlayParticles(data.transform.GetChild(0).GetComponent<ParticleSystem>());
                    } break;
                    
                    case VFXKind.Attack:
                    {
                        vfx.SetBase(AudioType.Enemy_Explosion, ShakeMode.Strong).ShockCamera(2, .1f).StopTime(.05f);
                    } break;
                };
            } break;
            
            case EntityType.NoEye:
            {
                switch (state)
                {
                    case VFXKind.Move:
                    {
                        vfx.FlashEntity(.8f, .07f, Color.red);
                    } break;
                }
            } break;
        }
        
        vfx.toggle += play;
        vfx.exit   += exit;
        Debug.Assert(vfx.exit == null || vfx.nextVFX == null);
        runner.InvokeAfterFrames(-1, () => PlayVFX(vfx, data, runner));
        
        bool IsDeathEffect() => state == VFXKind.Life && !start;
        bool IsSpawnEffect() => state == VFXKind.Life &&  start;
    }
    
    public static void PlayVFX(VFX vfx, VFXData data, MonoBehaviour coroutineRunner)
    {
        // Offset
        {
            Vector3 offset = vfx.offset;
            if (vfx.flags.HasProperty(VFXFlag.OffsetPos))
                ToggleOrLerp(p => data.transform.position   = p, Vector3.Lerp, data.transform.position,   data.transform.position   + offset);
            if (vfx.flags.HasProperty(VFXFlag.OffsetScale))
                ToggleOrLerp(s => data.transform.localScale = s, Vector3.Lerp, data.transform.localScale, data.transform.localScale + offset);
        }
        
        // Animation
        {
            if (vfx.flags.HasProperty(VFXFlag.StopAnimation))
                vfx.toggle += start => data.anim.speed = start ? 0 : 1;
            if (vfx.flags.HasProperty(VFXFlag.ChangeAnimation))
                data.anim.Play(vfx.text);
        }
        
        // Rotation
        vfx.toggle += start =>
        {
            if (start) return;
            Vector3 rotation = Vector3.zero;
            if (vfx.flags.HasProperty(VFXFlag.FlipX))
                rotation.x = 180;
            if (vfx.flags.HasProperty(VFXFlag.FlipY))
                rotation.y = 180;
            if (vfx.flags.HasProperty(VFXFlag.FlipZ))
                rotation.z = 180;
            data.transform.Rotate(rotation);
        };
        
        // Base
        {
            AudioManager.PlayAudio(vfx.audio);
            ObjectPooler.Spawn(vfx.pool, data.transform.position);
            
            if (vfx.flags.HasProperty(VFXFlag.StopTime))
                coroutineRunner.StartCoroutine(GameUtils.StopTime(vfx.duration));
            
            if (vfx.shake != ShakeMode.None)
                CameraSystem.instance.Shake(vfx.shake);
            if (vfx.flags.HasProperty(VFXFlag.ShockCamera))
                CameraSystem.instance.Shock(vfx.speed, vfx.size);
        }
        
        switch (vfx.type)
        {
            case VFXType.Camera:
            {
                if (vfx.flags.HasProperty(VFXFlag.Fade))
                    coroutineRunner.StartCoroutine(CameraSystem.instance.Flash(vfx.duration, vfx.color.a));
                else
                    Debug.Assert(false, "Only set type to Camera when you want to fade(flash) the camera");
            } break;
            
            case VFXType.Entity:
            {
                if (vfx.flags.HasProperty(VFXFlag.Fade))
                    ToggleOrLerp(a => data.sr.color = data.sr.color.ChangeAlpha(a), Mathf.Lerp, data.sr.color.a, vfx.color.a);
                else
                    coroutineRunner.StartCoroutine(Flashing(data.sr, data.whiteMat, vfx.color, vfx.duration, vfx.stayTime));
                
                static IEnumerator Flashing(SpriteRenderer sr, Material whiteMat, Color color, float duration, float flashTime)
                {
                    if (whiteMat == null) yield break;
                    whiteMat.color = color;
                    
                    while (duration > 0)
                    {
                        float currentTime = Time.time;
                        Material defMat = sr.material;
                        sr.material = whiteMat;
                        yield return new WaitForSeconds(flashTime);
                        
                        sr.material = defMat;
                        yield return new WaitForSeconds(flashTime);
                        duration -= Time.time - currentTime;
                    }
                    
                    whiteMat.color = Color.white;
                };
            } break;
            
            case VFXType.Trail:
            {
                coroutineRunner.StartCoroutine(EnableTrail(data.trail, vfx.duration - vfx.stayTime, vfx.stayTime));
                static IEnumerator EnableTrail(TrailRenderer trail, float emitTime, float stayTime)
                {
                    trail.enabled = true;
                    trail.emitting = true;
                    yield return new WaitForSeconds(emitTime);
                    
                    trail.emitting = false;
                    yield return new WaitForSeconds(stayTime);
                    
                    trail.Clear();
                    trail.emitting = true;
                    trail.enabled = false;
                }
            } break;
            
            case VFXType.Text:
            {
                if (vfx.flags.HasProperty(VFXFlag.Fade))
                    ToggleOrLerp(a => data.text.alpha = a, Mathf.Lerp, data.text.alpha, vfx.color.a);
                else if (vfx.color != Color.clear)
                    data.text.color = vfx.color;
                
                Debug.Assert(!vfx.flags.HasProperty(VFXFlag.ChangeAnimation));
                if (vfx.text != null)
                    data.text.text = vfx.text;
                
                if (vfx.size != 0 && !vfx.flags.HasProperty(VFXFlag.ShockCamera))
                    data.text.fontSize = vfx.size;
            } break;
        }
        
        vfx.toggle(true);
        System.Func<bool> condition = () => vfx.exit?.Invoke() != true;
        coroutineRunner.InvokeAfter(vfx.duration, () => vfx.toggle(false), condition);
        if (vfx.nextVFX != null)
            coroutineRunner.InvokeAfter(vfx.transitTime, () => PlayVFX(vfx.nextVFX, data, coroutineRunner), condition);
        else
            Debug.Assert(vfx.transitTime == 0);
        
        void ToggleOrLerp<T>(System.Action<T> setter, System.Func<T, T, float, T> lerp, T startValue, T endValue)
        {
            if (vfx.flags.HasProperty(VFXFlag.OverTime))
                coroutineRunner.StartCoroutine(ChangeOverTime((a, b, t) => setter(lerp(a, b, t)), startValue, endValue, vfx.duration));
            else
                vfx.toggle += start => setter(start ? endValue : startValue);;
            
            static IEnumerator ChangeOverTime(System.Action<T, T, float> setValue, T startValue, T endValue, float duration)
            {
                float startTime = Time.time;
                float endTime   = startTime + duration;
                while (Time.time < endTime)
                {
                    setValue(startValue, endValue, Mathf.InverseLerp(startTime, endTime, Time.time));
                    yield return null;
                }
                setValue(startValue, endValue, 1);
            }
        }
    }
    
    public enum VFXFlag
    {
        OverTime,
        StopTime,
        
        StopAnimation,
        ChangeAnimation,
        
        OffsetPos,
        OffsetScale,
        
        Fade,
        ShockCamera,
        
        FlipX,
        FlipY,
        FlipZ,
    }
    
    public enum VFXType
    {
        None,
        Camera,
        Entity,
        Trail,
        Text,
    }
    
    public class VFX
    {
        public VFX nextVFX;
        public float transitTime;
        
        public Property<VFXFlag> flags;
        public VFXType type;
        
        public System.Action<bool> toggle;
        public System.Func<bool> exit;
        
        public float duration;
        public float stayTime;
        public float size;
        public float speed;
        public Color color;
        
        public Vector2 offset;
        public string text;
        
        public ShakeMode shake;
        public AudioType audio;
        public PoolType pool;
        
        public void SetType(VFXType type)
        {
            Debug.Assert(duration == 0);
            Debug.Assert(this.type == VFXType.None);
            this.type = type;
        }
        
        public VFX SetBase(AudioType audioType, ShakeMode shakeMode = ShakeMode.None, PoolType poolType = PoolType.None)
        {
            audio = audioType;
            shake = shakeMode;
            pool = poolType;
            return this;
        }
        
        public VFX PlayParticles(params ParticleSystem[] particles)
        {
            toggle += start =>
            {
                if (start)
                    foreach (ParticleSystem particle in particles)
                    particle?.Play();
            };
            return this;
        }
        
        public VFX ToggleObjects(bool revert, params GameObject[] objs)
        {
            toggle += start =>
            {
                if (start || revert)
                    foreach (GameObject obj in objs)
                    obj.SetActive(!obj.activeSelf);
            };
            return this;
        }
        
        public VFX FadeEffect(float fadeTime, float alpha, VFXType type)
        {
            SetType(type);
            flags.SetProperty(VFXFlag.Fade, true);
            duration = fadeTime;
            color = new Color(1, 1, 1, alpha);
            return this;
        }
        
        public VFX FlashEntity(float duration, float stayTime, Color color)
        {
            SetType(VFXType.Entity);
            Debug.Assert(!flags.HasProperty(VFXFlag.Fade));
            this.duration = duration;
            this.stayTime = stayTime;
            this.color = color;
            return this;
        }
        
        public VFX EmitTrail(float emitTime, float stayTime)
        {
            SetType(VFXType.Trail);
            duration = emitTime;
            this.stayTime = stayTime;
            return this;
        }
        
        public VFX ShowText(string str, float fontSize, Color textColor)
        {
            SetType(VFXType.Text);
            Debug.Assert(size == 0);
            text = str;
            size = fontSize;
            color = textColor;
            return this;
        }
        
        public VFX PushNewVFX(float time = 0)
        {
            Debug.Assert(time >= -duration);
            transitTime = time;
            nextVFX = new VFX();
            return nextVFX;
        }
        
        public VFX StopTime(float duration)
        {
            Debug.Assert(duration == 0);
            this.duration = duration;
            flags.SetProperty(VFXFlag.StopTime, true);
            return this;
        }
        
        public VFX SetAnimation(string animation, bool stop = false)
        {
            text = animation;
            flags.SetProperty(VFXFlag.StopAnimation, stop);
            flags.SetProperty(VFXFlag.ChangeAnimation, true);
            return this;
        }
        
        public VFX SetOffset(Vector2 offset, float duration, bool scale = false, bool pos = false, bool overTime = false)
        {
            this.offset = offset;
            this.duration = duration;
            flags.SetProperty(VFXFlag.OffsetScale, scale);
            flags.SetProperty(VFXFlag.OffsetPos, pos);
            flags.SetProperty(VFXFlag.OverTime, overTime);
            return this;
        }
        
        public VFX ShockCamera(float shockSpeed, float shockSize)
        {
            Debug.Assert(size == 0);
            flags.SetProperty(VFXFlag.ShockCamera, true);
            speed = shockSpeed;
            size = shockSize;
            return this;
        }
        
        public VFX FlipObject(bool x = false, bool y = false, bool z = false)
        {
            flags.SetProperty(VFXFlag.FlipX, x);
            flags.SetProperty(VFXFlag.FlipY, y);
            flags.SetProperty(VFXFlag.FlipZ, z);
            return this;
        }
    }
    
#if !NEW_VFX
    public static void PlayVFX(EntityType type, VFXData data, VFXKind state, Vector2 dir, bool start,
                               System.Action<bool> play = null, System.Func<bool> exit = null)
    {
        EntityVFX vfx = null;
        switch (type)
        {
            case EntityType.Player:
            {
                Transform player = data.transform;
                ParticleSystem leftDust  = player.Find( "Left Dust").GetComponent<ParticleSystem>();
                ParticleSystem rightDust = player.Find("Right Dust").GetComponent<ParticleSystem>();
                switch (state)
                {
                    case VFXKind.Jump:
                    {
                        vfx = new EntityVFX
                        {
                            shakeMode = ShakeMode.PlayerJump,
                            waitTime = .25f,
                            particles = new ParticleSystem[] { dir.x >= 0 ? leftDust : null, dir.x <= 0 ? rightDust : null },
                            
                            audio = start ? AudioType.Player_Jump : AudioType.Player_Land,
                            scaleOffset = start ? new Vector2(-.25f, .25f) : new Vector2(.25f, -.25f),
                        };
                        
                        if (start)
                        {
                            vfx.rotateTime = .2f;
                            vfx.properties = new Property<VFXProperty>(VFXProperty.FlipX, VFXProperty.StopAnimation);
                            vfx.nextAnimation = "Idle";
                        }
                        else
                            vfx.nextAnimation = dir.x != 0 ? "Move" : "Idle";
                    } break;
                    
                    case VFXKind.Move:
                    {
                        vfx = new EntityVFX { };
                        if (Mathf.Abs(dir.x) == 1)
                        {
                            //AudioManager.ToggleAudio();
                            vfx.nextAnimation = start ? "Move" : "Idle";
                        }
                        
                        if (dir.x != 0) vfx.particles = new ParticleSystem[] { dir.x > 0 ? leftDust : rightDust };
                    } break;
                    
                    case VFXKind.Hurt:
                    {
                        vfx = new EntityVFX
                        {
                            properties = new Property<VFXProperty>(VFXProperty.StopAnimation, VFXProperty.ChangeEffectObjBack),
                            effectObj = player.Find("PlayerHitEffect").gameObject,
                            stopTime = .15f,
                            camFlashTime = .15f,
                            camFlashAlpha = .8f,
                            alpha = .5f,
                            fadeTime = .1f,
                            audio = AudioType.Player_Hurt,
                        };
                    } break;
                    
                    case VFXKind.Life:
                    {
                        if (!start) Debug.Assert(false, "TODO: IMPLEMENT DEATH EFFECT!");
                    } break;
                }
            } break;
            
            case EntityType.Bullet:
            {
                if (state == VFXKind.Life && !start)
                {
                    vfx = new EntityVFX
                    {
                        audio = AudioType.Weapon_Hit_Wall,
                        poolType = PoolType.VFX_Destroyed_Bullet,
                    };
                }
            } break;
            
            case EntityType.Weapon:
            {
                switch (state)
                {
                    case VFXKind.Attack:
                    {
                        if (start)
                        {
                            play(true);
                            vfx = new EntityVFX
                            {
                                audio = AudioType.Player_Shoot,
                                shakeMode = ShakeMode.GunKnockback,
                                //shakeDir = dir,
                                
                                properties = new Property<VFXProperty>(VFXProperty.ChangeEffectObjBack),
                                effectObj = data.transform.Find("MuzzleFlash").gameObject,
                                waitTime = 0.08f,
                                done = () => play(false),
                                canStop = exit,
                            };
                        }
                    } break;
                }
            } break;
            
            case EntityType.Maggot:
            {
                Debug.Assert(false, "TODO: NOT IMPLEMENTED");
            } break;
            
            case EntityType.NoEye:
            {
                Debug.Assert(false, "TODO: NOT IMPLEMENTED");
            } break;
            
            case EntityType.Cell:
            {
                AudioManager.PlayAudio(AudioType.Game_Pickup);
            } break;
        }
        
        PlayVFX(vfx, data, data.transform.GetComponent<MonoBehaviour>());
    }
    
    public static void PlayVFX(EntityVFX vfx, VFXData data, MonoBehaviour coroutineRunner)
    {
        if (vfx == null)
            return;
        
        if (vfx.properties.HasProperty(VFXProperty.ScaleOverTime))
            coroutineRunner.StartCoroutine(ScaleOverTime(data.transform, vfx.scaleTime, vfx.scaleOffset));
        else
            data.transform.localScale += (Vector3)vfx.scaleOffset;
        
        if (!string.IsNullOrEmpty(vfx.nextAnimation))
            data.anim.Play(vfx.nextAnimation);
        if (vfx.properties.HasProperty(VFXProperty.StopAnimation))
            data.anim.speed = 0;
        
        if (vfx.textColor != Color.clear)
            data.text.color = vfx.textColor;
        if (vfx.fontSize != 0)
            data.text.fontSize = vfx.fontSize;
        
        float totalParticleTime = 0;
        float particleCount = vfx.particles?.Length ?? 0;
        for (int i = 0; i < particleCount; ++i)
        {
            if (vfx.particles[i])
            {
                coroutineRunner.InvokeAfter(totalParticleTime, () => vfx.particles[i].Play(), true);
                if (vfx.properties.HasProperty(VFXProperty.PlayParticleInOrder))
                    totalParticleTime += vfx.particles[i].main.duration;
            }
        }
        
        if (vfx.effectObj)
            vfx.effectObj.SetActive(!vfx.effectObj.activeSelf);
        
        coroutineRunner.StartCoroutine(CameraSystem.instance.Flash(vfx.camFlashTime, vfx.camFlashAlpha));
        CameraSystem.instance.Shake(vfx.shakeMode, null);//, vfx.trauma == 0 ? 1 : vfx.trauma);
        CameraSystem.instance.Shock(vfx.shockSpeed, vfx.shockSize);
        
        AudioManager.PlayAudio(vfx.audio);
        ObjectPooler.Spawn(vfx.poolType, data.transform.position);
        ParticleEffect.instance.SpawnParticle(vfx.particleType, data.transform.position, vfx.range);
        
        coroutineRunner.StartCoroutine(GameUtils.StopTime(vfx.stopTime));
        coroutineRunner.StartCoroutine(Flashing(data.sr, data.whiteMat, vfx.triggerColor, vfx.flashDuration, vfx.flashTime, vfx.canStop));
        float x = vfx.properties.HasProperty(VFXProperty.FlipX) ? 180 : 0;
        float y = vfx.properties.HasProperty(VFXProperty.FlipY) ? 180 : 0;
        float z = vfx.properties.HasProperty(VFXProperty.FlipZ) ? 180 : 0;
        if (x != 0 || y != 0 || z != 0)
            coroutineRunner.InvokeAfter(vfx.rotateTime, () => data.transform.Rotate(new Vector3(x, y, z)));
        
        coroutineRunner.InvokeAfter(Mathf.Max(vfx.flashDuration, totalParticleTime, vfx.scaleTime), () =>
                                    {
                                        if (vfx.properties.HasProperty(VFXProperty.FadeTextWhenDone))
                                            coroutineRunner.StartCoroutine(FadeText(data.text, vfx.alpha, vfx.fadeTime));
                                        else
                                            coroutineRunner.StartCoroutine(Flashing(data.sr, data.whiteMat, new Color(1, 1, 1, vfx.alpha), vfx.fadeTime, vfx.fadeTime, vfx.canStop));
                                        
                                        if (vfx.properties.HasProperty(VFXProperty.StartTrailing))
                                            coroutineRunner.StartCoroutine(EnableTrail(data.trail, vfx.trailEmitTime, vfx.trailStayTime));
                                        if (vfx.properties.HasProperty(VFXProperty.DecreaseTrailWidth))
                                            coroutineRunner.InvokeAfter(vfx.trailEmitTime, () => coroutineRunner.StartCoroutine(DecreaseTrailWidth(data.trail, vfx.trailStayTime)));
                                        
                                        coroutineRunner.InvokeAfter(Mathf.Max(vfx.fadeTime, vfx.trailEmitTime + vfx.trailStayTime) + vfx.waitTime, () =>
                                                                    {
                                                                        if (!vfx.properties.HasProperty(VFXProperty.ScaleOverTime))
                                                                            data.transform.localScale -= (Vector3)vfx.scaleOffset;
                                                                        if (vfx.properties.HasProperty(VFXProperty.ChangeEffectObjBack))
                                                                            vfx.effectObj.SetActive(!vfx.effectObj.activeSelf);
                                                                        if (data.anim)
                                                                            data.anim.speed = 1;
                                                                        vfx.done?.Invoke();
                                                                    });
                                    });
        
        static IEnumerator Flashing(SpriteRenderer sr, Material whiteMat, Color color, float duration, float flashTime, System.Func<bool> canStop)
        {
            if (whiteMat == null)
                yield break;
            
            whiteMat.color = color;
            
            while (duration > 0)
            {
                if (canStop?.Invoke() ?? false)
                    break;
                
                float currentTime = Time.time;
                Material defMat = sr.material;
                sr.material = whiteMat;
                yield return new WaitForSeconds(flashTime);
                
                sr.material = defMat;
                yield return new WaitForSeconds(flashTime);
                duration -= Time.time - currentTime;
            }
            
            whiteMat.color = Color.white;
        }
        
        static IEnumerator ScaleOverTime(Transform transform, float duration, Vector3 scaleOffset)
        {
            while (duration > 0)
            {
                duration -= Time.deltaTime;
                transform.localScale += scaleOffset * Time.deltaTime;
                yield return null;
            }
            
            while (transform.gameObject.activeSelf)
            {
                transform.localScale -= scaleOffset * Time.deltaTime;
                yield return null;
            }
        }
        
        static IEnumerator FadeText(TMPro.TextMeshPro text, float alpha, float fadeTime)
        {
            float dAlpha = (text.alpha - alpha) / fadeTime;
            while (text.alpha > alpha)
            {
                text.alpha -= dAlpha * Time.deltaTime;
                yield return null;
            }
        }
        
        static IEnumerator EnableTrail(TrailRenderer trail, float emitTime, float stayTime)
        {
            trail.enabled = true;
            trail.emitting = true;
            yield return new WaitForSeconds(emitTime);
            
            trail.emitting = false;
            yield return new WaitForSeconds(stayTime);
            
            trail.Clear();
            trail.emitting = true;
            trail.enabled = false;
        }
        
        static IEnumerator DecreaseTrailWidth(TrailRenderer trail, float decreaseTime)
        {
            float startWidth = trail.widthMultiplier;
            float startTime = decreaseTime;
            while (decreaseTime > 0)
            {
                trail.widthMultiplier = decreaseTime / startTime * startWidth;
                decreaseTime -= Time.deltaTime;
                yield return null;
            }
            trail.widthMultiplier = startWidth;
        }
    }
#endif
    
}

public enum VFXProperty
{
    StopAnimation,
    ChangeEffectObjBack,
    ScaleOverTime,
    FadeTextWhenDone,
    StartTrailing,
    DecreaseTrailWidth,
    PlayParticleInOrder,
    FlipX,
    FlipY,
    FlipZ,
}

[System.Serializable]
public class EntityVFX
{
    public Property<VFXProperty> properties;
    
    public System.Action done;
    public System.Func<bool> canStop;
    public string nextAnimation;
    public GameObject effectObj;
    public ParticleSystem[] particles;
    
    [Header("Time")]
    public float waitTime;
    public float scaleTime;
    public float rotateTime;
    
    [Header("Trail Effect")]
    public float trailEmitTime;
    public float trailStayTime;
    
    [Header("Flashing")]
    public float flashTime;
    public float flashDuration;
    public Color triggerColor;
    
    [Header("Text Effect")]
    public Color textColor;
    public float fontSize;
    
    [Header("Camera Effect")]
    public float stopTime;
    public float trauma;
    public ShakeMode shakeMode;
    public float shockSpeed;
    public float shockSize;
    public float camFlashTime;
    public float camFlashAlpha;
    
    [Header("After Fade")]
    public float alpha;
    public float fadeTime;
    
    [Header("Explode Particle")]
    public float range;
    public ParticleType particleType;
    
    [Header("Other")]
    public AudioType audio;
    public PoolType poolType;
    public Vector2 scaleOffset;
}

/*void PlayVFX(EntityVFX vfx)
{
    // Sample Code

    // Player hurt
    EntityVFX vfx = new EntityVFX
    {
        stopTime = .15f,
        flashDuration = .1f,
        flashTime = .1f,
        triggerColor = Color.white,
        camFlashTime = .15f,
        camFlashAlpha = .8f,
        stopAnimation = true,
        //effectObj = hitEffect, revertObj = true
    };

    // Player death
    vfx = new EntityVFX
    {
        audio = AudioType.Player_Death,
        nextAnimation = "Death",
        effectObj = transform.GetChild(0).gameObject,
        stopTime = 2,
        particles = new ParticleSystem[] { deathBurstParticle, deathFlowParticle },
    };

    // Player flip
    transform.position -= GetPosOnGround();
    vfx = new EntityVFX
    {
        shakeMode = ShakeMode.Medium,
        audio = AudioType.Player_Jump,
        scaleOffset = new Vector2(-.25f, .25f),
        duration = .2f,
        particles = checking() ? ...
    };

    // Bullet hit effect
    ObjectPooler.Spawn(damagePopup, collision.transform.position, Quaternion.identity).GetComponent<MovingEntity>().InitDamagePopup(damage);
    vfx = new EntityVFX
    {
        poolType = PoolType.VFX_Destroyed_Bullet,
        audio = AudioType.Weapon_Hit_Wall,
        effectObj = gameObject,
    };

    // Explosion
    vfx = new EntityVFX
    {
        audio = AudioType.Enemy_Explosion,
        shakeMode = ShakeMode.Strong,
        smoothFunc = MathUtils.SmoothStart3,
        shockSpeed = 2,
        shockSize = .1f,
        particleType = ParticleType.Explosion, range = explodeRange,
        stopTime = .05f,
    };
    if (IsInRange(explodeRange))
        player.Hurt(abilityDamage);
    int dropValue = moneyDrop.randomValue;
    for (int i = 0; i < dropValue; i++)
        ObjectPooler.Spawn(PoolType.Cell, transform.position, Quaternion.identity);
    numberOfEnemiesAlive--;
    Destroy(gameObject);

    // Enemy Death
    vfx = new EntityVFX
    {
        shakeMode = ShakeMode.Medium,
        poolType = PoolType.VFX_Destroyed_Enemy,
        stopTime = .05f,
        audio = AudioType.Enemy_Death,
        // TODO: Spawn splash of blood and small pieces
    };
    int dropValue = moneyDrop.randomValue;
    for (int i = 0; i < dropValue; i++)
        ObjectPooler.Spawn(PoolType.Cell, transform.position, Quaternion.identity);
    numberOfEnemiesAlive--;
    Destroy(gameObject);

    // Enemy Hurt
    vfx = new EntityVFX
    {
        audio = AudioType.Player_Hurt,
        stopTime = .02f,
        flashDuration = .1f,
        flashTime = .1f,
        triggerColor = hurtColor,
    };

    // Damage popup
    vfx = new EntityVFX
    {
        duration = 1,
        scaleOffset = Vector2.one * .5f,
        alpha = 3,
        done = ,
    };

    // ----------------------------------------

    GameInput.EnableAllInputs(false);
    controller.audioManager.PlayAudio(AudioType.Player_Death);
    anim.Play("Death");
    transform.GetChild(0).gameObject.SetActive(false);
    yield return new WaitForSeconds(.5f);

    Time.timeScale = 0;
    deathBurstParticle.Play();
    yield return new WaitForSecondsRealtime(2);

    Time.timeScale = 1;
    deathFlowParticle.Play();
    yield return new WaitForSeconds(deathFlowParticle.main.duration);
    // TODO: Replay and enable all inputs

    // ----------------------------------------

    anim.speed = 0;
    sr.material = hurtMat;
    hitEffect.SetActive(true);
    transform.localScale = new Vector2(.75f, 1f);

    Time.timeScale = 0f;
    StartCoroutine(cam.Flash(.15f, .8f));
    yield return new WaitForSecondsRealtime(.15f);
    Time.timeScale = 1f;

    yield return new WaitForSeconds(.1f);
    sr.material = defMat;
    hitEffect.SetActive(false);
    transform.localScale = new Vector2(1f, 1f);

    Color temp = sr.color;
    temp.a = invincibleOpacity;
    sr.color = temp;

    yield return new WaitForSeconds(invincibleTime);

    temp.a = 1;
    sr.color = temp;
    anim.speed = 1;

    // ----------------------------------------

    CameraShake.instance?.Shake(ShakeMode.Medium, trauma: .4f);
    PlayDust(-moveInput);
    audioManager.PlayAudio(isJumping ? AudioType.Player_Jump : AudioType.Player_Land);

    // Change Size
    transform.localScale = isJumping ? new Vector3(.75f, 1.25f) : new Vector3(1.25f, .75f);
    transform.position -= GetPosOnGround();

    StopCoroutine(resetSize);
    resetSize = this.InvokeAfter(.2f, () => {
        transform.localScale = new Vector3(1f, 1f);
        if (!isJumping)
            transform.position -= GetPosOnGround();
    });

    Vector3 GetPosOnGround()
    {
        float groundHeight = Physics2D.BoxCast(transform.position, new Vector2(spriteExtents.x / 2, 0.01f), 0, -transform.up, spriteExtents.y * 2,
                LayerMask.GetMask("Ground")).distance;
        Vector3 offset = new Vector3(0, groundHeight - spriteExtents.y * transform.localScale.y) * transform.up.y;
        Debug.DrawRay(transform.position, -transform.up * groundHeight, Color.blue);
        return offset;
    }

    // ----------------------------------------

    if (spawnDamagePopup)
        ObjectPooler.Spawn(damagePopup, collision.transform.position, Quaternion.identity).GetComponent<MovingEntity>().InitDamagePopup(damage);
    ObjectPooler.Spawn(PoolType.VFX_Destroyed_Bullet, transform.position, Quaternion.identity);
    audioManager.PlayAudio(AudioType.Weapon_Hit_Wall);
    gameObject.SetActive(false);

    // ----------------------------------------

    Charge();
    yield return new WaitForSeconds(abilityChargeTime);

    audioManager.PlaySfx(abilitySound);
    CameraShake.instance.Shake(cameraShakeMode, MathUtils.SmoothStart3);
    CameraShake.instance.Shock(2);
    // TODO: Change ParticleEffect to a singleton
    FindObjectOfType<ParticleEffect>().SpawnParticle(ParticleType.Explosion, transform.position, explodeRange);
    if (IsInRange(explodeRange))
        player.Hurt(abilityDamage);
    Die(true);

    // ----------------------------------------

    whiteMat.color = color;
    while (duration > 0)
    {
        float currentTime = Time.time;

        sr.material = whiteMat;
        yield return new WaitForSeconds(flashTime);

        sr.material = defMat;
        yield return new WaitForSeconds(flashTime);

        duration -= Time.time - currentTime;
    }

    // ----------------------------------------

    if (!explode)
    {
        CameraShake.instance.Shake(ShakeMode.Medium);
        ObjectPooler.Spawn(PoolType.VFX_Destroyed_Enemy, transform.position, Quaternion.identity);
    }

    // TODO: Spawn splash of blood and small pieces

    player.InvokeAfter(.3f, () => player.StartCoroutine(GameUtils.StopTime(.05f)));
    audioManager.PlayAudio(AudioType.Enemy_Death);
    int dropValue = moneyDrop.randomValue;
    for (int i = 0; i < dropValue; i++)
        ObjectPooler.Spawn(PoolType.Cell, transform.position, Quaternion.identity);
    numberOfEnemiesAlive--;
    Destroy(gameObject);

    // ----------------------------------------

    if (state == EnemyState.Invincible)
        return;
    // TODO: Have different hurt sound for enemies.
    audioManager.PlayAudio(AudioType.Player_Hurt);
    health.value -= damage;
    if (health.value > 0f)
        StartCoroutine(GameUtils.StopTime(.02f));
    StartCoroutine(Flashing(.1f, .1f, hurtColor));
}*/