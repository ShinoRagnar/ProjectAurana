using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundWhen
{
    ShieldHit = 1,
    HullHit = 2,
    Shooting = 3,
    JetPack = 4,
    Exploding = 5,
    GearChangeRolling = 6,
    GearChangeWalking = 7,
    Footsteps = 8,
    Engine = 9,
    Landing = 10,
    Braking = 11,
    TransformingSpider = 12,
    TransformingRoller = 13,
    GearOne = 14,
    GearTwo =15,
    GearThree =16,
    GearFour =17,
    //Inventory
    PickUp =18,
    PutDown =19,
    Blocked =20,
    InventoryOpen =21,
    EquipmentOpen =22,
    InventoryClose =23,
    EquipmentClose =24,
    Tooltip =25,
    CornerHit = 50,
    Detach = 60,
    Vibration = 70,
    Swing = 80,
    FleshHit = 90,
    Block = 91,
    HitEffect = 100
    
   
}
public class AudioContainer : IGameClone<AudioContainer>{

    private Dictionary<SoundWhen, List<Sound>> sounds;
    //private AudioListener listener;

    public bool HasSound(SoundWhen snd)
    {
        return sounds.ContainsKey(snd);
    }

    public AudioContainer(AudioContainerData acd) : this()
    {
        foreach(AudioData ad in acd.audioFiles)
        {
            AddSound(ad.when, Global.Resources[ad.audio], ad.volume, ad.pitch);
        }
    }
	public AudioContainer()
    {
       // this.listener = listenerVal;
        this.sounds = new Dictionary<SoundWhen, List<Sound>>();
    }
    public bool PlaySound(SoundWhen when, AudioSource source, bool randomizePitch)
    {
        if (source != null)
        {
            //Debug.Log("playing sound: " + when);
            if (sounds.ContainsKey(when))
            {
                source.PlayOneShot(InitSource(sounds[when].RandomElement(), source, randomizePitch).clip);
            }
            else
            {
                Debug.Log("Couldnt find sound: " + when);
                return false;
            }
        }
        else
        {
            Debug.Log("Playing on disabled source");
            return false;
        }
        return true;
    }
    public bool PlaySound(SoundWhen when, AudioSource source, float volume)
    {
        if (source != null)
        {
            //Debug.Log("playing sound: " + when);
            if (sounds.ContainsKey(when))
            {
                source.PlayOneShot(InitSource(sounds[when].RandomElement(), source, false, true, volume).clip);
            }
            else
            {
                Debug.Log("Couldnt find sound: " + when);
                return false;
            }
        }
        else
        {
            Debug.Log("Playing on disabled source");
            return false;
        }

        return true;
    }
    public bool PlaySound(SoundWhen when, AudioSource source, bool randomizePitch, float startTime)
    {
        if(source != null)
        {
            if (sounds.ContainsKey(when))
            {
                source.clip = InitSource(sounds[when].RandomElement(), source, randomizePitch).clip;
                source.PlayScheduled(AudioSettings.dspTime + startTime);
            }
            else
            {
                Debug.Log("Couldnt find sound: " + when);
                return false;
            }
        }
        else
        {
            Debug.Log("Playing sound on disabled source");
            return false;
        }

        return true;
    }
    public void PlaySoundContinuously(SoundWhen when, AudioSource source, bool randomizePitch)
    {
        if(source != null)
        {
            if (sounds.ContainsKey(when))
            {
                source.clip = InitSource(sounds[when].RandomElement(), source, randomizePitch).clip;
                source.Play();
                source.playOnAwake = true;
                source.loop = true;
            }
        }
        else
        {
            Debug.Log("Trying to play on disabled source");
        }

    }


    private Sound InitSource(Sound toPlay, AudioSource source, bool randomizePitch, bool useVolume = false, float volume = 0)
    {
        if(source != null)
        {
            source.spatialBlend = 0.5f;
            if (randomizePitch)
            {
                source.pitch = (float)Global.instance.rand.NextDouble() * toPlay.pitch / 2 + toPlay.pitch / 2;
            }
            else
            {
                source.pitch = toPlay.pitch;
            }
            if (useVolume)
            {
                source.volume = volume; //(float)Global.instance.rand.NextDouble() * toPlay.volume / 2 + toPlay.volume / 2;
            }
            else
            {
                source.volume = toPlay.volume;
            }
        }
        return toPlay;
    }

    public void AddSound(SoundWhen soundWhen, AudioClip[] clips, float volume, float pitch)
    {
        List<Sound> list = new List<Sound>();
        foreach(AudioClip clip in clips)
        {
            list.Add(new Sound(clip, volume, pitch));
        }
        AddSound(soundWhen, list);
    }
    public void AddSound(SoundWhen soundWhen, Sound sound)
    {
        if (!sounds.ContainsKey(soundWhen))
        {
            sounds.Add(soundWhen, new List<Sound> { { sound } });
        }
        else
        {
            sounds[soundWhen].Add(sound);
        }
    }
    public void AddSound(SoundWhen soundWhen, List<Sound> sound)
    {
        if (!sounds.ContainsKey(soundWhen))
        {
            sounds.Add(soundWhen, sound);
        }
    }
    public AudioContainer Clone()
    {
        AudioContainer ret = new AudioContainer();
        foreach(SoundWhen sw in sounds.Keys)
        {
            foreach(Sound s in sounds[sw])
            {
                ret.AddSound(sw,s.Clone());
            }
        }
        return ret;
    }

}
