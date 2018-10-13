using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Stat
{
    //Movement
    WalkSpeed = 0,
    WalkAcceleration = 1,
    RollSpeed = 2,
    RollAcceleration = 3,

    //Jumping
    JumpForce = 4,

    //Stats
    Health = 5,
    Shield = 6,

    //Physics
    Weight = 7,
    EyePosition = 8,
    
    //Moveability
    ActionSpeed = 10,
    Moveability = 12,

    //Combat
    DamageDealt = 60,
    Radius = 70,
    ReloadSpeed = 80,
    EMPDamageDealt = 90,
    CorruptionDamageDealt = 100,
    OnHitDamageDealt = 110,

    //Damage received
    DamageReceived = 200,
    EMPDamageReceived = 210,
    CorruptionDamageReceived = 220,
    OnHitDamageReceived = 230,
    Armor = 240

}
public class Stats {

    //Max values, example: max health
    //<Stat,float>
    private Dictionary<int, float> stats;
    //Percentage of the stat, example: current health
    private Dictionary<int, float> values;


    /*private Dictionary<int, float> ttt = new Dictionary<int, float>()
    {
        {1,1 },
        {3,1 }
    };

    private struct Test : System.IEquatable<Test>{

        Stat stat;
        int hashcode;

        public Test(Stat s)
        {
            stat = s;
            hashcode = s.GetHashCode();
        }
        public override int GetHashCode()
        {
            return hashcode;
        }
        public bool Equals(Test other)
        {
            return false;
        }
        public override bool Equals(object other)
        {
            return false;
        }

    }*/

    //Stat s = Stat.RollAcceleration.

    //Save calculations to spare cpu cycles    Condition
    private Dictionary<int, Dictionary<ListHash<int>, float>> preCalcTime;
    private Dictionary<int, Dictionary<ListHash<int>, float>> preCalcValue;

    public GameUnit owner;
    //private Dictionary<StatsAffector, Buff> buffs;

    public float cacheHit = 0;
    public float cacheMiss = 0;

    public Stats()
    {
        this.stats = new Dictionary<int, float>();
        this.values = new Dictionary<int, float>();
        InitPrecalc();
    }

    public Stats(
        //Common
        float maxHealth, 
        float maxShield, 
        float weight, 
        float eyePosition, 
        //Only for moving 
        float walkSpeed = 0, 
        float walkAcceleration = 0,
        //Only for vehicles
        float rollSpeed = 0, 
        float rollAcceleration = 0
        )
    {
        this.stats = new Dictionary<int, float>
        {
            { (int)Stat.WalkSpeed, walkSpeed },
            { (int)Stat.RollSpeed, rollSpeed },
            { (int)Stat.RollAcceleration, rollAcceleration },
            { (int)Stat.WalkAcceleration, walkAcceleration },
            { (int)Stat.Health, maxHealth },
            { (int)Stat.Weight, weight },
            { (int)Stat.Shield, maxShield },
            { (int)Stat.EyePosition, eyePosition },
            { (int)Stat.ActionSpeed, 1 },
            { (int)Stat.Moveability, 1 },
            { (int)Stat.DamageDealt, 1 },
            { (int)Stat.Radius, 1 },
            { (int)Stat.ReloadSpeed, 1 },
            { (int)Stat.EMPDamageDealt, 1 },
            { (int)Stat.CorruptionDamageDealt, 1 },
            { (int)Stat.OnHitDamageDealt, 1 },
            { (int)Stat.DamageReceived, 1 },
            { (int)Stat.EMPDamageReceived, 1 },
            { (int)Stat.CorruptionDamageReceived, 1 },
            { (int)Stat.OnHitDamageReceived, 1 },
            { (int)Stat.Armor, 1 }
         //   { (int)Stat.ShieldBlockRadius, 1 }
        };

        this.values = new Dictionary<int, float>
        {
            { (int)Stat.WalkSpeed, 0.4f },//Start at 0.3 and go to max
            { (int)Stat.WalkAcceleration, 1 },
            { (int)Stat.RollAcceleration, 1},
            { (int)Stat.RollSpeed, 0 }, //Start at 0 and go to max
            { (int)Stat.Health, 1 },
            { (int)Stat.Weight, 1 },
            { (int)Stat.Shield, 1 },
            { (int)Stat.EyePosition, 1 },
            { (int)Stat.ActionSpeed, 1 },
            { (int)Stat.Moveability, 1 },
            { (int)Stat.DamageDealt, 1 },
            { (int)Stat.Radius, 1 },
            { (int)Stat.ReloadSpeed, 1 },
            { (int)Stat.EMPDamageDealt, 1 },
            { (int)Stat.CorruptionDamageDealt, 1 },
            { (int)Stat.OnHitDamageDealt, 1 },
            { (int)Stat.DamageReceived, 1 },
            { (int)Stat.EMPDamageReceived, 1 },
            { (int)Stat.CorruptionDamageReceived, 1 },
            { (int)Stat.OnHitDamageReceived, 1 },
            { (int)Stat.Armor, 1 }
        };

        InitPrecalc();
        //this.buffs = new Dictionary<StatsAffector, Buff>();
    }

    

    private void InitPrecalc()
    {
        this.preCalcTime = new Dictionary<int, Dictionary<ListHash<int>, float>>();
        this.preCalcValue = new Dictionary<int, Dictionary<ListHash<int>, float>>();
    }

    //Example use to damage health
    public float Damage(Stat stat, float amount)
    {
        if (stats.ContainsKey((int)stat))
        {
            float max = GetStat(stat, Global.ALWAYS);
            float dam = GetValuePercentage(stat) * max - amount;
            values[(int)stat] = Mathf.Max(dam, 0) / max;
            //Spill over damage
            if(dam < 0)
            {
                return -dam;
            }
        }
        return 0;
    }
    //Sets a stat to 0
    //Test t = new Test(Stat.JumpForce);

    public void Kill(Stat stat)
    {
        //ttt.ContainsKey((int)stat);
        if (stats.ContainsKey((int)stat))
        {
            values[(int)stat] = 0;
        }
    }

    //Sets a stat to a certain percentage
    public void SetValuePercentage(Stat stat, float value)
    {
        if (stats.ContainsKey((int)stat))
        {
            values[(int)stat] = value;
        }
    }
    //Example use to heal health
    public float Heal(Stat stat, float amount)
    {
        if (stats.ContainsKey((int)stat))
        {
            float max = GetStat(stat, Global.ALWAYS);
            float oldVal = values[(int)stat];
            float newVal = Mathf.Min(GetValuePercentage(stat) * max + amount, max) / max;
            values[(int)stat] = newVal;
            return oldVal - newVal;
        }
        return 0;
    }
    // Example: Use this to get current health
    public float GetCurrentValue(Stat stat)
    {
        return GetValuePercentage(stat) * GetStat(stat, Global.ALWAYS);
    }
    // Example: Use this to get current health
    public float GetCurrentValue(Stat stat, ListHash<int> conditions)
    {
        return GetValuePercentage(stat) * GetStat(stat, conditions);
    }
    // Example: use this to get current health percentage 0-1
    public float GetValuePercentage(Stat stat)
    {
        if (stats.ContainsKey((int)stat))
        {
            return values[(int)stat];
        }
        return 0;
    }
    public float GetStat(Stat stat)
    {
        return GetStat(stat, Global.ALWAYS);
    }
    // Example: Use this to get Max Health or Move Speed
    public float GetStat(Stat stat, ListHash<int> conditions)
    {
        //Debug
        if (!stats.ContainsKey((int)stat) || owner == null)
        {
            return 0;
        }
        //For gameobjects without buffs
        if(owner.buffHandler == null)
        {
            return stats[(int)stat];
        }
        
        //See if we have already calculated this stat
        bool contained = false;
        if (preCalcTime.ContainsKey((int)stat))
        {
            if (preCalcTime[(int)stat].ContainsKey(conditions))
            {
                if(preCalcTime[(int)stat][conditions] > owner.buffHandler.statUpdateTime.StatUpdatedAt((int)stat))
                {
                    cacheHit++;
                    return preCalcValue[(int)stat][conditions];
                }
            }
            contained = true;
        }
        float ret = GetStatInner(stat, conditions);

        //Add calculation so we can get it later
        if (!contained)
        {
            preCalcTime.Add((int)stat, new Dictionary<ListHash<int>, float> { { conditions, Time.time } });
            preCalcValue.Add((int)stat, new Dictionary<ListHash<int>, float> { { conditions, ret } });
        }
        else
        {
            if (preCalcTime[(int)stat].ContainsKey(conditions))
            {
                preCalcTime[(int)stat][conditions] = Time.time;
                preCalcValue[(int)stat][conditions] = ret;
            }
            else
            {
                preCalcTime[(int)stat].Add( conditions, Time.time);
                preCalcValue[(int)stat].Add( conditions, ret);

            }
        }
        cacheMiss++;
        return ret;
    }

    private float GetStatInner(Stat stat, ListHash<int> conditions)
    {
        float ret = 0;

        if (stats.ContainsKey((int)stat))
        {
            float unModified = stats[(int)stat];

            if (owner.buffHandler != null)
            {
                float additives = 1;
                float multiplicatives = 1;
                float unique = 0;

                foreach (Buff b in owner.buffHandler.buffs)
                {
                    if (b.isActive) {
                        foreach (StatsAffector affector in b.affectors)
                        {
                            if(affector.affecting == stat) { 
                                if (affector.CheckConditions(conditions)) //affector.condition == Condition.Always || conditions.Contains((int)affector.condition))
                                {

                                    if (affector.calculation == Calculation.Additive)
                                    {
                                        float add = affector.magnitude * b.stacks.Count;

                                        if(affector.cap < 0)
                                        {
                                            add = Mathf.Max(add, affector.cap);
                                        }else if (affector.cap > 0)
                                        {
                                            add = Mathf.Min(add, affector.cap);
                                        }
                                        additives += add;
                                    }
                                    else if (affector.calculation == Calculation.Multiplicative)
                                    {
                                        float mul = Mathf.Pow(affector.magnitude, b.stacks.Count);
                                        if (affector.cap < 0)
                                        {
                                            mul = Mathf.Max(mul, affector.cap);
                                        }
                                        else if (affector.cap > 0)
                                        {
                                            mul = Mathf.Min(mul, affector.cap);
                                        }
                                        multiplicatives *= mul;
                                    }
                                    else if (affector.calculation == Calculation.UniqueModifier)
                                    {
                                        return unique;
                                    }
                                }
                            }
                        }
                    }
                }

                return Mathf.Max(unModified * additives * multiplicatives,0);

            }
            else
            {
                return Mathf.Max(unModified,0);
            }

      
        }
        return ret;
    }




    public void AddStat(Stat stat, float magnitude)
    {
        if (stats.ContainsKey((int)stat))
        {
            stats[(int)stat] = magnitude;
            values[(int)stat] = 1;
        }
        else
        {
            stats.Add((int)stat, magnitude);
            values.Add((int)stat, 1);
        }
    }
    public Stats Clone()
    {
        Stats ret = new Stats();
        foreach(Stat s in stats.Keys)
        {
            ret.AddStat(s, stats[(int)s]);
        }
        return ret;

    }
	
}
