using System;
using UnityEngine;

// NOTE: Because OnEnable was called before Application.isPlaying got updated so I had to call the init logic in AudioObject.cs
//       Hopefully, It will get fixed in the next update
[CreateAssetMenu(menuName = "Audio/AudioManager")]
public class AudioManager : ScriptableObject
{
    public Sound[] sounds;
    private AudioSource musicSource;
    private AudioSource sfxSource;

    public static AudioManager instance;

    public void Init()
    {
        if (!instance)
        {
            instance = this;
            GameObject audioObject = new GameObject("AudioManager");
            audioObject.hideFlags = HideFlags.HideAndDontSave;
            instance.musicSource = audioObject.AddComponent<AudioSource>();
            instance.sfxSource = audioObject.AddComponent<AudioSource>();
            foreach (Sound s in AudioManager.instance.sounds)
            {
                if (s.playOnAwake)
                {
                    PlayMusic(s.name);
                }
            }
        }
    }

    public void PlaySfx(SoundCue cue)
    {
        if (Time.time >= cue.timeBtwSoundsValue)
        {
            PlaySfx(cue.sounds[UnityEngine.Random.Range(0, cue.sounds.Length)], cue.volumeScale.randomValue, cue.pitch.randomValue);
            cue.timeBtwSoundsValue = Time.time + cue.timeBtwSounds;
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

    /// <summary>Play sound effect using the PlayOneShot method from a separate AudioSource</summary>
    /// <param name="volumeScale">percent of the current volume (0-1)</param>
    public void PlaySfx(string name, float volumeScale = 1, float pitch = 1)
    {
        Sound sound = GetSound(name);
        if (sound != null)
        {
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(GetSound(name).clip, volumeScale); 
        }
    }

    /// <summary>
    /// Play the music using the Play method from a separate AudioSource.
    /// </summary>
    public void PlayMusic(string name)
    {
        Sound s = GetSound(name);
        musicSource.clip = s.clip;
        musicSource.volume = s.volume;
        musicSource.pitch = s.pitch;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public Sound GetSound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            InternalDebug.LogWarning("Sound: " + name + " not found!");
        }
        return s;
    }

    public void PauseAllSounds()
    {
        AudioListener.pause = true;
    }

    public void ResumeAllSounds()
    {
        AudioListener.pause = false;
    }
}
