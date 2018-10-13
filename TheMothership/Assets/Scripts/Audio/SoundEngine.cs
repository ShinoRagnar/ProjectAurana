using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Adds engine sound, walk sounds etc. Used for the legs
public class SoundEngine {

    public static float MAX_ENGINE_SOUND = 0.2f;
    public static float GEARING_DURATION = 0.25f;
    public static float GEAR_DECAY_FACTOR = 1f;

    //General audio comppenent for "playOnce"
    public AudioSource general;
    //Used for jetpack constant sound
    public AudioSource jetpack;
    //Used for engine constant sound
    public AudioSource engine;
    //Used when feet are put down
    public AudioSource footsteps;
    //Used when switching gears
    public AudioSource gearing;
    //Used when transforming between states
    public AudioSource transforming;

    // Continuous Audio mixing
    /*public AudioSource engineGearOne;
    public AudioSource engineGearTwo;
    public AudioSource engineGearThree;
    public AudioSource engineGearFinal;*/

    GameUnit owner;

    public SoundEngine(GameUnit ownr)
    {
        owner = ownr;

        general = owner.body.gameObject.AddComponent<AudioSource>();
        jetpack = owner.body.gameObject.AddComponent<AudioSource>();
        engine = owner.body.gameObject.AddComponent<AudioSource>();
        footsteps = owner.body.gameObject.AddComponent<AudioSource>();
        gearing = owner.body.gameObject.AddComponent<AudioSource>();
        transforming = owner.body.gameObject.AddComponent<AudioSource>();
    }

    public void Footsteps()
    {
        owner.mech.legs.sounds.PlaySound(SoundWhen.Footsteps,footsteps, false);
    }
    public void TransformToRoller()
    {
        owner.mech.legs.sounds.PlaySound(SoundWhen.TransformingRoller, transforming, false, 0.5f);
        transforming.volume = 0.5f;
        engine.volume = 0.2f;
    }
    public void TransformToSpider()
    {
        owner.mech.legs.sounds.PlaySound(SoundWhen.TransformingSpider, transforming, false, 0.5f);
        transforming.volume = 0.5f;
        engine.volume = 0;
    }
    public void GearChange(bool spidermode)
    {
        if (!spidermode) {
            owner.mech.legs.sounds.PlaySound(spidermode ? SoundWhen.GearChangeWalking : SoundWhen.GearChangeRolling, gearing, false);
            gearing.volume = 0.4f;
        }
    }
    public void Land(float velocity)
    {
        //Debug.Log(velocity);
        float volume = Mathf.Min((Mathf.Max(velocity-6,0) / 15), 1)*0.15f;
        //Debug.Log(volume);
        owner.mech.legs.sounds.PlaySound(SoundWhen.Landing, general, volume);
    }

}
