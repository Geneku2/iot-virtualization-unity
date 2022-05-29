using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] soundsList;

    void Awake()
    {
        foreach (Sound currSound in soundsList) {
            if(currSound.objectSource != null)
                currSound.source = currSound.objectSource.AddComponent<AudioSource>();
            else
                currSound.source = gameObject.AddComponent<AudioSource>();
            currSound.source.clip = currSound.clip;
            currSound.source.volume = currSound.volume;
            currSound.source.pitch = currSound.pitch;
            currSound.source.loop = currSound.loop;
        }
    }

    public void PlaySound (string name)
    {
        Sound currSound = Array.Find(soundsList, sound => sound.name == name);
        if(currSound == null) {
            Debug.LogWarning("Sound: " + name + " does not exist");
        };
        currSound.source.Play();
    }
    public void StopSound (string name)
    {
        Sound currSound = Array.Find(soundsList, sound => sound.name == name);
        if(currSound == null) {
            Debug.LogWarning("Sound: " + name + " does not exist");
        };
        currSound.source.Stop();
    }
}
