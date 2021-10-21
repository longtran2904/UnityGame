using UnityEngine;

[CreateAssetMenu(menuName = "Audio/SoundCue")]
public class SoundCue : ScriptableObject
{
    public string[] sounds;
    [MinMax( 0f, 1f)] public RangedFloat volumeScale;
    [MinMax(-3f, 3f)] public RangedFloat pitch;
    public float timeBtwSounds;
    [HideInInspector] public float timeBtwSoundsValue;
}
