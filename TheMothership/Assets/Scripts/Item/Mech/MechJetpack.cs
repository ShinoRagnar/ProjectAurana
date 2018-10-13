using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechJetpack{

    public static readonly float TIME_UNTIL_FULL_LENGTH_BEAM = 1;
    public static readonly float TIME_UNTIL_FULL_LENGTH_BEAM_EXHAUST = 3;
    public static readonly float CENTER_BEAM_LENGTH = 0.3f;
    public static readonly float OTHER_BEAM_LENGTH = 0.4f;
    public static readonly float EXHAUST_BEAM_LENGTH = 1.5f;
    public static readonly float EXHAUST_DECAY_FACTOR = 10;
    public static readonly float BEAM_WIDTH = 0.3f;
    public static readonly float BEAM_WIDTH_EXHAYST = 1f;

    //                      PointOfInterest
    public DictionaryList<int, Item> beams = new DictionaryList<int, Item>();
    public DictionaryList<int, Item> exhausts = new DictionaryList<int, Item>();

    //public List<PointOfInterest> centerBeams = new List<PointOfInterest>();
    public List<int> backBeams = new List<int>();
    public List<int> frontBeams = new List<int>();

    public List<int> exhaustFrontBeams = new List<int>();
    public List<int> exhaustBackBeams = new List<int>();


    public AudioSource source;
    public AudioSource exhaustFront;
    public AudioSource exhaustBack;

    public GameUnit owner;

    //Jetpack
    float timeShowingBeam = 0;
    float timeUsingFrontJet = 0;
    float timeUsingBackJet = 0;

    //Exhausts
    float timeUsingFrontExhaust = 0;
    float timeUsingBackExhaust = 0;

    bool beamsShowing = false;
    bool exhaustsShowing = false;

    public MechJetpack(GameUnit ownerVal, Item jetpackvisuals, Item exhaustVisuals)
    {
        this.owner = ownerVal;

        Alignment alig = new Alignment(0, 0, 0, 0, 0, 0, -1+ BEAM_WIDTH, -1 + BEAM_WIDTH, -1 + BEAM_WIDTH);
        Alignment aligExhaust = new Alignment(0, 0, 0, 0, 0, 0, -1 + BEAM_WIDTH_EXHAYST, -1 + BEAM_WIDTH_EXHAYST, -1 + BEAM_WIDTH_EXHAYST);

        beams.Add((int)PointOfInterest.BackLeftFoot1, jetpackvisuals.CloneNewAlignment(alig));
        beams.Add((int)PointOfInterest.BackLeftFoot2, jetpackvisuals.CloneNewAlignment(alig));
        beams.Add((int)PointOfInterest.BackLeftFoot3, jetpackvisuals.CloneNewAlignment(alig));

        beams.Add((int)PointOfInterest.BackRightFoot1, jetpackvisuals.CloneNewAlignment(alig));
        beams.Add((int)PointOfInterest.BackRightFoot2, jetpackvisuals.CloneNewAlignment(alig));
        beams.Add((int)PointOfInterest.BackRightFoot3, jetpackvisuals.CloneNewAlignment(alig));

        beams.Add((int)PointOfInterest.FrontLeftFoot1, jetpackvisuals.CloneNewAlignment(alig));
        beams.Add((int)PointOfInterest.FrontLeftFoot2, jetpackvisuals.CloneNewAlignment(alig));
        beams.Add((int)PointOfInterest.FrontLeftFoot3, jetpackvisuals.CloneNewAlignment(alig));

        beams.Add((int)PointOfInterest.FrontRightFoot1, jetpackvisuals.CloneNewAlignment(alig));
        beams.Add((int)PointOfInterest.FrontRightFoot2, jetpackvisuals.CloneNewAlignment(alig));
        beams.Add((int)PointOfInterest.FrontRightFoot3, jetpackvisuals.CloneNewAlignment(alig));

        //Exhaust pipes
        exhausts.Add((int)PointOfInterest.ExhaustFront1, exhaustVisuals.CloneNewAlignment(aligExhaust));
        exhausts.Add((int)PointOfInterest.ExhaustFront2, exhaustVisuals.CloneNewAlignment(aligExhaust));
        exhausts.Add((int)PointOfInterest.ExhaustBack1, exhaustVisuals.CloneNewAlignment(aligExhaust));
        exhausts.Add((int)PointOfInterest.ExhaustBack2, exhaustVisuals.CloneNewAlignment(aligExhaust));


        backBeams.AddRange(new int[] {
            (int)PointOfInterest.BackLeftFoot1,
            (int)PointOfInterest.BackRightFoot1,
            (int)PointOfInterest.BackLeftFoot2,
            (int)PointOfInterest.BackLeftFoot3,
            (int)PointOfInterest.BackRightFoot2,
            (int)PointOfInterest.BackRightFoot3 });

        frontBeams.AddRange(new int[] {
            (int)PointOfInterest.FrontRightFoot2,
            (int)PointOfInterest.FrontRightFoot3,
            (int)PointOfInterest.FrontLeftFoot2,
            (int)PointOfInterest.FrontLeftFoot3,
            (int)PointOfInterest.FrontLeftFoot1,
            (int)PointOfInterest.FrontRightFoot1 });

        exhaustFrontBeams.AddRange(new int[] {
            (int)PointOfInterest.ExhaustFront1,
            (int)PointOfInterest.ExhaustFront2});

        exhaustBackBeams.AddRange(new int[] {
            (int)PointOfInterest.ExhaustBack1,
            (int)PointOfInterest.ExhaustBack2});

    }
    public void HideAndUnequip()
    {

        foreach (int pos in beams)
        {
            Item i = beams[pos];
            owner.itemEquiper.Unequip(i);
            i.Hide();
        }
        foreach (int pos in exhausts)
        {
            Item i = exhausts[pos];
            owner.itemEquiper.Unequip(i);
            i.Hide();
        }
    }

    public void ShowAndEquipAndPlaySounds(Legs leg)
    {
        
        foreach (int pos in beams)
        {
            Item i = beams[pos];
            owner.itemEquiper.Equip(i);
            i.Show(leg.GetPointOfInterest((PointOfInterest)pos));
            if(pos == (int)PointOfInterest.FrontRightFoot1)
            {
                source = i.visualItem.gameObject.AddComponent<AudioSource>();
                leg.sounds.PlaySoundContinuously(SoundWhen.JetPack, source, false);
            }
            i.Disable();
        }
        foreach (int pos in exhausts)
        {
            Item i = exhausts[pos];
            owner.itemEquiper.Equip(i);
            i.Show(leg.GetPointOfInterest((PointOfInterest)pos));
            if(pos == (int)PointOfInterest.ExhaustFront1)
            {
                exhaustFront = i.visualItem.gameObject.AddComponent<AudioSource>();
                leg.sounds.PlaySoundContinuously(SoundWhen.Engine, exhaustFront, false);
            }
            else if (pos == (int)PointOfInterest.ExhaustBack1)
            {
                exhaustBack = i.visualItem.gameObject.AddComponent<AudioSource>();
                leg.sounds.PlaySoundContinuously(SoundWhen.Engine, exhaustBack, false);
            }
            i.Disable();
        }
        leg.jetpack = this;

        //Beam sounds and engine sounds
        /*source = beams[(int)PointOfInterest.FrontRightFoot1].visualItem.gameObject.AddComponent<AudioSource>();
        exhaustFront = exhausts[(int)PointOfInterest.ExhaustFront1].visualItem.gameObject.AddComponent<AudioSource>();
        exhaustBack = exhausts[(int)PointOfInterest.ExhaustBack1].visualItem.gameObject.AddComponent<AudioSource>();*/
    }

    public void ShowBeams(float heading, Facing facing, bool power)
    {

        if (!beamsShowing) { 
            foreach (int pos in beams)
            {
                Item i = beams[pos];
                i.Enable();
            }
        }
        source.volume = timeShowingBeam;

        if (power)
        {
            timeShowingBeam += Time.deltaTime;
            
            //Calculate time heading in direction
            if (heading < 0)
            {
                if (facing == Facing.Left)
                {
                    timeUsingFrontJet -= Time.deltaTime;
                    timeUsingBackJet += Time.deltaTime;

                }
                else if (facing == Facing.Right)
                {
                    timeUsingFrontJet += Time.deltaTime;
                    timeUsingBackJet -= Time.deltaTime;
                }
            }
            else if (heading > 0)
            {
                if (facing == Facing.Right)
                {
                    timeUsingBackJet += Time.deltaTime;
                    timeUsingFrontJet -= Time.deltaTime;
                }
                else if (facing == Facing.Left)
                {
                    timeUsingBackJet -= Time.deltaTime;
                    timeUsingFrontJet += Time.deltaTime;
                }
            }
            else
            {
                timeUsingFrontJet += Time.deltaTime;
                timeUsingBackJet += Time.deltaTime;
            }
        }
        else
        {
            timeUsingFrontJet -= Time.deltaTime;
            timeUsingBackJet -= Time.deltaTime;
            timeShowingBeam -= Time.deltaTime;
        }


        Clamp();

        //Percentage
        float backPercent = timeUsingBackJet / TIME_UNTIL_FULL_LENGTH_BEAM;
        float frontPercent = timeUsingFrontJet / TIME_UNTIL_FULL_LENGTH_BEAM;
        float percentage = timeShowingBeam / TIME_UNTIL_FULL_LENGTH_BEAM;

        ChangeBackBeamLength(percentage * OTHER_BEAM_LENGTH/2+backPercent*OTHER_BEAM_LENGTH/2);
        ChangeFrontBeamLength(percentage * OTHER_BEAM_LENGTH/2+frontPercent*OTHER_BEAM_LENGTH/2);

        beamsShowing = true;
    }

    public void ShowExhausts(float heading, Facing facing, bool power)
    {

        if (!exhaustsShowing)
        {
            foreach (int pos in exhausts)
            {
                Item i = exhausts[pos];
                i.Enable();
            }
        }

        if (power)
        {
            //Calculate time heading in direction
            if (heading < 0)
            {
                if (facing == Facing.Left)
                {
                    timeUsingFrontExhaust -= Time.deltaTime* EXHAUST_DECAY_FACTOR;
                    timeUsingBackExhaust += Time.deltaTime;

                }
                else if (facing == Facing.Right)
                {
                    timeUsingFrontExhaust += Time.deltaTime;
                    timeUsingBackExhaust -= Time.deltaTime* EXHAUST_DECAY_FACTOR;
                }
            }
            else if (heading > 0)
            {
                if (facing == Facing.Right)
                {
                    timeUsingBackExhaust += Time.deltaTime;
                    timeUsingFrontExhaust -= Time.deltaTime* EXHAUST_DECAY_FACTOR;
                }
                else if (facing == Facing.Left)
                {
                    timeUsingBackExhaust -= Time.deltaTime* EXHAUST_DECAY_FACTOR;
                    timeUsingFrontExhaust += Time.deltaTime;
                }
            }
            else
            {
                timeUsingFrontExhaust -= Time.deltaTime* EXHAUST_DECAY_FACTOR;
                timeUsingBackExhaust -= Time.deltaTime * EXHAUST_DECAY_FACTOR;
            }
        }
        else
        {
            timeUsingFrontExhaust -= Time.deltaTime* EXHAUST_DECAY_FACTOR;
            timeUsingBackExhaust -= Time.deltaTime* EXHAUST_DECAY_FACTOR;
        }


        timeUsingBackExhaust = timeUsingBackExhaust > TIME_UNTIL_FULL_LENGTH_BEAM_EXHAUST ? TIME_UNTIL_FULL_LENGTH_BEAM_EXHAUST : timeUsingBackExhaust;
        timeUsingBackExhaust = timeUsingBackExhaust < 0 ? 0 : timeUsingBackExhaust;
        timeUsingFrontExhaust = timeUsingFrontExhaust > TIME_UNTIL_FULL_LENGTH_BEAM_EXHAUST ? TIME_UNTIL_FULL_LENGTH_BEAM_EXHAUST : timeUsingFrontExhaust;
        timeUsingFrontExhaust = timeUsingFrontExhaust < 0 ? 0 : timeUsingFrontExhaust;

        //Percentage
        float backPercent = timeUsingBackExhaust / TIME_UNTIL_FULL_LENGTH_BEAM_EXHAUST;
        float frontPercent = timeUsingFrontExhaust / TIME_UNTIL_FULL_LENGTH_BEAM_EXHAUST;

        exhaustFront.volume = frontPercent*0.05f;
        exhaustBack.volume = backPercent*0.05f;

        ChangeBackExhaustLength(backPercent * EXHAUST_BEAM_LENGTH);
        ChangeFrontExhaustLength(frontPercent * EXHAUST_BEAM_LENGTH);

        exhaustsShowing = true;
    }

    public void Clamp()
    {
        //Clamp 0-1
        timeUsingBackJet = timeUsingBackJet > TIME_UNTIL_FULL_LENGTH_BEAM ? TIME_UNTIL_FULL_LENGTH_BEAM : timeUsingBackJet;
        timeUsingBackJet = timeUsingBackJet < 0 ? 0 : timeUsingBackJet;
        timeUsingFrontJet = timeUsingFrontJet > TIME_UNTIL_FULL_LENGTH_BEAM ? TIME_UNTIL_FULL_LENGTH_BEAM : timeUsingFrontJet;
        timeUsingFrontJet = timeUsingFrontJet < 0 ? 0 : timeUsingFrontJet;
        timeShowingBeam = timeShowingBeam > TIME_UNTIL_FULL_LENGTH_BEAM ? TIME_UNTIL_FULL_LENGTH_BEAM : timeShowingBeam;
        timeShowingBeam = timeShowingBeam < 0 ? 0 : timeShowingBeam;

    }

    private void ChangeBackExhaustLength(float length)
    {
        foreach (int pos in exhaustBackBeams)
        {
            Transform t = exhausts[pos].visualItem;
            t.localScale = new Vector3(t.localScale.x, t.localScale.y, length);
        }
    }
    private void ChangeFrontExhaustLength(float length)
    {
        foreach (int pos in exhaustFrontBeams)
        {
            Transform t = exhausts[pos].visualItem;
            t.localScale = new Vector3(t.localScale.x, t.localScale.y, length);
        }
    }

    private void ChangeBackBeamLength(float length)
    {
        foreach (int pos in backBeams)
        {
            Transform t = beams[pos].visualItem;
            t.localScale = new Vector3(t.localScale.x, t.localScale.y, length);
        }
    }
    private void ChangeFrontBeamLength(float length)
    {
        foreach (int pos in frontBeams)
        {
            Transform t = beams[pos].visualItem;
            t.localScale = new Vector3(t.localScale.x, t.localScale.y, length);
        }
    }

    public void HideExhausts()
    {
        timeUsingFrontExhaust = 0;
        timeUsingBackExhaust = 0;
        if (exhaustsShowing)
        {
            foreach (int pos in exhausts)
            {
                Item i = exhausts[pos];
                i.Disable();
            }
            //Set audio start time to new random point
            exhaustBack.time = Random.Range(exhaustBack.clip.length / 8, exhaustBack.clip.length / 2);
            exhaustFront.time = Random.Range(exhaustFront.clip.length / 8, exhaustFront.clip.length / 2);
        }
        exhaustsShowing = false;
    }

    public void HideBeams()
    {
        timeShowingBeam = 0;
        timeUsingFrontJet = 0;
        timeUsingBackJet = 0;
        if (beamsShowing)
        {
            foreach (int pos in beams)
            {
                Item i = beams[pos];
                i.Disable();
            }
            //Set audio start time to new random point
            source.time = Random.Range(source.clip.length / 8, source.clip.length/2);
        }
        beamsShowing = false;
    }


}
