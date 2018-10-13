using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Condition
{
    Always = 0,
    MovingRight = 1,
    MovingLeft = 2,
    Jumping = 3,
    OnGround = 4,
    //Combat
    AttackingWithAMeleeWeapon = 100,
    AttackingWithAAxe=101,
    AttackingWithASword = 102,
    AttackingWithAHammer = 103,
    AttackingWithASpear = 104,
    ShootingWithARangedWeapon = 110,
    ShootingWithARifle = 120,
    ShootingWithAMissileLauncher = 130,
    ShootingWithAMortar = 140,
    Blocking = 145
}
public enum Calculation
{
    Additive,
    Multiplicative,
    UniqueModifier
}
public class StatsAffector{

    public Calculation calculation;
    public Stat affecting;
    public Condition[] condition;
    public float cap;
    public float magnitude;
    public string name;
    public bool alwaysApplies = false;


    public StatsAffector(string uniqueName, Stat affectingVal, Condition[] conditionVal, Calculation calc, float magnitudeVal, float capVal)
    {
        this.name = uniqueName;
        this.calculation = calc;
        this.affecting = affectingVal;
        this.condition = conditionVal;
        this.magnitude = magnitudeVal;
        this.cap = capVal;

        foreach(Condition c in conditionVal)
        {
            if(c == Condition.Always)
            {
                alwaysApplies = true;
            }
        }
    }
    public bool CheckConditions(ListHash<int> conditions)
    {
        if (alwaysApplies) { return true; }
        else {
            foreach (Condition c in condition) {
                if (!conditions.Contains((int)c))
                {
                    return false;
                }
            }
        }
        return true;
    }


    public StatsAffector Clone()
    {
        return new StatsAffector(name, affecting, condition, calculation, magnitude, cap);
    }



}
