using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 3f)]
    public float volume = 1;
    [Range(0f, 3f)]
    public float pitch = 1;
    public bool loop;
    public bool playOnAwake;
}