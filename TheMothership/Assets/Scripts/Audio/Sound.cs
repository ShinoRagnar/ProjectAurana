using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound {

    public AudioClip clip;
    public float volume;
    public float pitch;

    public Sound(AudioClip audioClipVal, float volumeVal, float PitchVal)
    {
        this.clip = audioClipVal;
        this.volume = volumeVal;
        this.pitch = PitchVal;
        
    }

    public Sound Clone()
    {
        return new Sound(clip, volume, pitch);
    }
}
