using System;
using UnityEngine;

public enum AudioType
{
    Player_Jump,
    Player_Shoot,
    Player_Hurt,
    Player_Footstep,
    Player_Land,
    Player_Death,
    Player_DefeatBoss,

    Player_Count,

    Turret_Shoot,
    Enemy_Explosion,
    Enemy_Death,
    Boss_Explosion,
    Boss_Hit_Wall,
    Boss_Dash,

    Enemy_Count,

    Weapon_Shotgun,
    Weapon_Bounce,
    Weapon_Trickshot,
    Weapon_Hit_Wall,

    Weapon_Count,

    Game_Select,
    Game_Buy,
    Game_Pickup,
    Game_OpenChest,

    Game_Count,

    Music_Main,
    Music_Boss,

    Music_Count,

    Audio_Count
}

[CreateAssetMenu(menuName = "Audio/AudioManager")]
public class AudioManager : ScriptableObject
{
    [Serializable]
    public struct Audio
    {
        public AudioType type;
        public AudioClip[] clips;
        [MinMax(0f, 3f)] public RangedFloat volume;
        [MinMax(0f, 3f)] public RangedFloat pitch;
    }

    public Sound[] sounds;
    public Audio[] audios;
    private AudioSource sfxSource;

    private int[] soundTypes = {
        (int)AudioType.Player_Count,
        (int)AudioType.Enemy_Count,
        (int)AudioType.Weapon_Count,
        (int)AudioType.Game_Count,
        (int)AudioType.Music_Count
    };

    private AudioSource[] sources;

    static AudioManager instance;

    private void OnEnable()
    {
#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) Init();
#else
        if (Application.isPlaying) Init();
#endif
    }

    void Init()
    {
        if (!instance)
        {
            instance = this;
            GameObject audioObject = new GameObject("AudioManager");
            //audioObject.hideFlags = HideFlags.HideAndDontSave;
            instance.sfxSource = audioObject.AddComponent<AudioSource>();
            sources = new AudioSource[soundTypes.Length];
            for (int i = 0; i < sources.Length; i++)
            {
                sources[i] = audioObject.AddComponent<AudioSource>();
            }
        }
    }

    /// <summary>Play sound effect using the PlayOneShot method from a separate AudioSource</summary>
    public void PlaySfx(string name)
    {
        Sound sound = GetSound(name);
        if (sound != null)
        {
            sfxSource.pitch = sound.pitch;
            sfxSource.PlayOneShot(sound.clip, sound.volume); 
        }
    }

    private Sound GetSound(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
            InternalDebug.LogWarning("Sound: " + name + " not found!");
        return s;
    }

    private int GetSoundType(AudioType type)
    {
        int t = (int)type;
        for (int i = 0; i < soundTypes.Length; ++i)
        {
            if (t < soundTypes[i])
                return i;
        }
        return -1;
    }

    public void PlayAudio(AudioType type)
    {
        int t = GetSoundType(type);
        Audio s = audios[(int)type - t];
        sources[t].pitch = s.pitch.randomValue;
        if (soundTypes[t] == (int)AudioType.Music_Count)
        {
            sources[t].clip = s.clips.RandomElement();
            sources[t].volume = s.volume.randomValue;
            sources[t].Play();
        }
        else
        {
            sources[t].PlayOneShot(s.clips.RandomElement(), s.volume.randomValue);
        }
    }
}
