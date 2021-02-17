using UnityEngine;

[CreateAssetMenu(menuName = "Audio/SoundCue")]
public class SoundCue : ScriptableObject
{
    public string[] sounds;
    [Range(0f, 1f)] public float volumeScaleMin = 1;
    [Range(0f, 1f)] public float volumeScaleMax = 1;
    public float pitchMin = 1;
    public float pitchMax = 1;
    public float timeBtwSounds;
    [HideInInspector] public float timeBtwSoundsValue;
}
