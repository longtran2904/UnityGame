using System;
using UnityEngine;

public enum AudioType
{
    None,
    Player_Jump,
    Player_Shoot,
    Player_Hurt,
    Player_Footstep,
    Player_Land,
    Player_Death,
    Player_DefeatBoss,

    Turret_Shoot,
    Enemy_Explosion,
    Enemy_Death,
    Boss_Explosion,
    Boss_Hit_Wall,
    Boss_Dash,

    Weapon_Shotgun,
    Weapon_Bounce,
    Weapon_Trickshot,
    Weapon_Hit_Wall,

    Game_Select,
    Game_Buy,
    Game_Pickup,
    Game_OpenChest,

    Music_Main,
    Music_Boss,

    Audio_Count
}

[Serializable]
public struct Audio
{
    public AudioType type;
    public AudioClip[] clips;
    [MinMax(0f, 3f)] public RangedFloat volume;
    [MinMax(0f, 3f)] public RangedFloat pitch;
}

public static class AudioManager
{
    private static AudioSource[] sources;
    private static Audio[] audios;
    private static AudioType firstMusic;

    public static void Init(GameObject obj, Audio[] audios, AudioType firstMusic, int sourceCount)
    {
        sources = new AudioSource[sourceCount];
        for (int i = 0; i < sourceCount; i++)
            sources[i] = obj.AddComponent<AudioSource>();
        AudioManager.audios = audios;
        AudioManager.firstMusic = firstMusic;
    }

    public static void PlayAudio(AudioType type)
    {
        if (type == AudioType.None)
            return;
        Audio audio = audios[(int)type - 1];
        Debug.Assert(audio.type == type, $"AudioType {type} isn't matched with {audio.type}!");

        float pitch = audio.pitch.randomValue;
        foreach (AudioSource source in sources)
        {
            if (source.pitch == pitch || !source.isPlaying)
            {
                source.pitch = pitch;
                if (type == firstMusic)
                    source.Play(audio.clips.RandomElement(), audio.volume.randomValue);
                else
                    source.PlayOneShot(audio.clips.RandomElement(), audio.volume.randomValue);
                return;
            }
        }
        Debug.LogWarning($"Can't find a valid audio source for {type}!");
    }
}
