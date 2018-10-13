using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BuffType
{
    SingleDurationInstance = 10,
    DurationPerStack = 20,
    NoDuration = 30,
    Passive = 40
}
public class Buff : IGameClone<Buff>{




    //public List<StatsAffector> affect;
    //private Dictionary<string, StatsAffector> affectors;

    public ListDictionary<string, StatsAffector> affectors = new ListDictionary<string, StatsAffector>();
    public ListDictionary<MechItem, Stack> stacks = new ListDictionary<MechItem, Stack>();
    public ListHash<Stat> affects = new ListHash<Stat>();
    public ListHash<OnHit.ActiveEffect> effects = new ListHash<OnHit.ActiveEffect>();

    //public List<Stack> stacks;
    //public Dictionary<string, Stack> containsStack;

    //public HashSet<Stat> doesAffect;
    //public List<Stat> affects;

    //public TextMeshProUGUI text;
    public float duration;
    public string buffName;
    public float currentDuration;
    //public int currentStacks;
    /*public Transform prefabStatics;
    public Transform prefabStacks;
    public Transform prefabDuration;


    public Transform visibleStatics;
    public Transform visibleStacks;
    public Transform visibleDurations;

    public RectTransform stacksRectTransform;
    public RectTransform staticsRectTransform;
    public RectTransform durationRectTransform;

    public RectTransform timeBar;*/

    public UIContainer ui;
    public Sprite sprite;

    public bool isDebuff;
    public bool isActive;
    public bool updatedStackLastFrame = false;

    public BuffType buffType;
    public BuffNames origin;

    public Buff(BuffData data, BuffNames origin) : this(data.buffName, data.duration, Global.Resources[data.buffPicture], data.isDebuff, data.type, origin)
    {
        foreach(StatsAffectorData sad in data.affectors)
        {
            AddAffector(new StatsAffector(sad.statAffectorName, sad.stat, sad.condition, sad.calculation, sad.amount, sad.threshold));
        }
    }
    public Buff(string buffnameVal, float durati, Sprite icn /*Transform icn*/, bool isDebuffVal, BuffType buffTypeVal, BuffNames origin)
    {
        this.origin = origin;
        this.buffName = buffnameVal;
        this.duration = durati;
        this.buffType = buffTypeVal;
        this.sprite = icn;

        //Transform buffFrame = Global.Create(Global.Resources[PrefabNames.BuffFrame],Global.References[SceneReferenceNames.PanelDebuffStatic]);
        //Global.FindDeepChild(buffFrame, "Image").GetComponent<Image>().sprite = icn;

        if(buffTypeVal == BuffType.NoDuration)
        {
            /*ui = new UIContainer(
                buffnameVal,
                new Transform[] { Global.References[SceneReferenceNames.PanelDebuffStatic] }, //Global.instance.PANEL_DEBUFF[0] },
                new Transform[] { Global.Resources[PrefabNames.BuffFrame] },
                new Sprite[] { icn });
                */
            ui = new UIContainer(
               buffnameVal,
               new Transform[]{
                       Global.References[SceneReferenceNames.PanelDebuffStatic],
                       Global.References[SceneReferenceNames.PanelDebuffStack],
                     //  Global.References[SceneReferenceNames.PanelDebuffDuration]
               }, //Global.instance.PANEL_DEBUFF,
               new Transform[] {
                       Global.Resources[PrefabNames.BuffFrame],
                       Global.Resources[PrefabNames.BuffStack],
                    //   Global.Resources[PrefabNames.BuffDuration]
                   //Global.instance.UI_STACKS,
                   //Global.instance.UI_DURATION
               },
               new Sprite[] { sprite },null,
              
               /*new Transform[] {
                        Global.Resources[PrefabNames.BuffDuration]
               }*/
               new string[] { Global.NAME_TIME_BAR },
               new Transform[] {
                        Global.Resources[PrefabNames.BuffStack]
               },
               FadeDirection.FadeUp
               );
        }
        else
        {
            ui = new UIContainer(
                    buffnameVal,
                    new Transform[]{
                       Global.References[SceneReferenceNames.PanelDebuffStatic],
                       Global.References[SceneReferenceNames.PanelDebuffStack],
                       Global.References[SceneReferenceNames.PanelDebuffDuration]
                    }, //Global.instance.PANEL_DEBUFF,
                    new Transform[] {
                       Global.Resources[PrefabNames.BuffFrame],
                       Global.Resources[PrefabNames.BuffStack],
                       Global.Resources[PrefabNames.BuffDuration]
                        //Global.instance.UI_STACKS,
                        //Global.instance.UI_DURATION
                    },
                    new Sprite[] {sprite},
                    new Transform[] {
                        Global.Resources[PrefabNames.BuffDuration]
                        //Global.instance.UI_DURATION 
                    },
                    new string[] { Global.NAME_TIME_BAR },
                    new Transform[] {
                        Global.Resources[PrefabNames.BuffStack]
                        //Global.instance.UI_STACKS
                    },
                    FadeDirection.FadeUp
                    );
        }

        /*this.prefabStatics = icn;
        this.prefabStacks = Organizer.instance.UI_STACKS;
        this.prefabDuration = Organizer.instance.UI_DURATION;*/
        this.isDebuff = isDebuffVal;
        this.isActive = true;
        this.currentDuration = 0;
        //this.stacks = new List<Stack>();
        //this.containsStack = new Dictionary<string, Stack>();
        //this.affectors = new Dictionary<string, StatsAffector>();
        //this.doesAffect = new HashSet<Stat>();
        //this.affects = new List<Stat>();
        //this.affect = new List<StatsAffector>();
    }
    public void AddAffector(StatsAffector aff)
    {
        affectors.AddIfNotContains(aff.name, aff);
        affects.AddIfNotContains(aff.affecting);

        /*
        if (!affectors.Contains(aff.name))
        {
            affectors.Add(aff.name,aff);
            if (!doesAffect.Contains(aff.affecting))
            {
                doesAffect.Add(aff.affecting);
                affects.Add(aff.affecting);
            }
            //affect.Add(aff);
        }*/
    }
    public Buff Clone()
    {
        Buff clone = new Buff(buffName, duration, sprite/*ui.prefabs[0]*/, isDebuff, buffType, origin);
        foreach(StatsAffector stats in affectors)
        {
            clone.AddAffector(stats);
        }
        return clone;

    }
    public void UpdateDurationPerStack()
    {
      //  List<Stack> toRemove = new List<Stack>();
        foreach(Stack s in stacks)
        {
            s.currentDuration += Time.deltaTime;
            if(buffType == BuffType.DurationPerStack)
            {
                if(s.currentDuration > duration)
                {
                    updatedStackLastFrame = true;
                    stacks.RemoveLater(s.source); //s.name);
                }
            }
        }
        stacks.Remove();
    }
    
    public void DurationEnded(GameUnit owner)
    {
        //Debug.Log("Buff duration ended:"+ this.buffName);
        this.isActive = false;
        this.stacks.Clear();

        foreach(OnHit.ActiveEffect ae in effects)
        {
            ae.source.ReturnEffect(owner, ae.toReturn, ae.effect);

            if (owner.activeEffecs.Contains(ae))
            {
                owner.activeEffecs.Remove(ae);
            }
        }
        effects.Clear();

        //this.containsStack.Clear();
    }
    public void RemoveStack(MechItem mi)//string source)
    {
        if (stacks.Contains(mi))//source))
        {
            stacks.Remove(mi);
            updatedStackLastFrame = true;
        }
    }

    //Returns true if this turned the buff on
    public bool AddStack(MechItem mi)//string source)
    {
        this.currentDuration = 0;
        bool wasActive = this.isActive;
        updatedStackLastFrame = true;
        //Debug.Log("Adding stack: " + source + " for " + this.buffName);

        if (stacks.Contains(mi))//source))
        {
            stacks[mi].currentDuration = 0;
            // stacks[source].currentDuration = 0;
        }
        else
        {
            stacks.Add(mi, new Stack(mi, 0));
            //stacks.Add(source, new Stack(source, 0));

        }
        this.isActive = true;
        return wasActive;
    }


}
