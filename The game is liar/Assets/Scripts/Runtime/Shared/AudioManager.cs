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
    private static float[] pitches;

    public static void Init(GameObject obj, Audio[] audios, AudioType firstMusic, int sourceCount)
    {
        sources = new AudioSource[sourceCount];
        pitches = new float[sourceCount];
        for (int i = 0; i < sourceCount; i++)
            sources[i] = obj.AddComponent<AudioSource>();
        AudioManager.audios = audios;
        AudioManager.firstMusic = firstMusic;
        /*foreach (Audio audio in audios)
        {
            for (int i = 0; i < audio.clips.Length; ++i)
            {
                AudioClip clip = audio.clips[i];
                AudioClip newClip = AudioClip.Create(clip.name, clip.samples, clip.channels, clip.frequency, true, OnAudioRead);
                float[] data = new float[clip.samples];
                if (clip.GetData(data, 0))
                    newClip.SetData(data, 0);
                audio.clips[i] = newClip;

                static void OnAudioRead(float[] data)
                {
                    Debug.Log("Reading");
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (data[i] != 0)
                            Debug.Log(data[i]);
                        data[i] *= 1;
                    }
                }
            }
        }*/
    }

    // RANT: I want to do something fundamental here: I want to play a sound with a randomized volume and pitch.
    // That's it. A straightforward and common thing to do in games.
    // But I can't because Unity doesn't provide any function to play a sound with a different pitch (PlayOneShot only takes in a volume argument).
    // Here are a couple of ways to do this:
    // 1. Create a pool of audio source which is memory intensive
    // 2. Create a new clip everytime (memory intensive)
    // 3. Use PCMReaderCallback (haven't tested yet because of this https://issuetracker.unity3d.com/issues/cannot-get-data-from-streamed-samples-when-using-pcm-read-callbacks)
    // 4. Use OnAudioFilterRead
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

    public static void ReadAudio(float[] data, int channels)
    {
        for (int i = 0; i < data.Length; i++)
            data[i] = 0f;
    }
}
