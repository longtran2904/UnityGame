using UnityEngine;

public enum VFXType
{
    None,
    Camera,
    Flash,
    Fade,
    Trail,
    Text,
}

public enum VFXFlag
{
    StopAnimation,
    ResumeAnimation,
    StopTime,
    ToggleCurrent,
    
    OffsetPosition,
    OffsetScale,
    OverTime,
    
    ScaleOut,
    FadeOut,
    
    FlipX,
    FlipY,
    FlipZ,
}

[CreateAssetMenu(fileName = "VFX", menuName = "Data/VFX")]
public class VFX : ScriptableObject
{
    public Property<VFXFlag> flags;
    public VFXType type;
    
    public RangedFloat timeline;
    public Color color;
    public float stayTime;
    public float speed;
    public float size;
    
    public ParticleType particleType;
    public ShakeMode shakeMode;
    public AudioType audio;
    public PoolType[] pools;
    
    public string animation;
    public Vector2 offset;
}
