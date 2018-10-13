using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum InteractionComparator
{
    Is = 0,
    Below = 10,
    Above = 20
}
public enum InteractionCalculation
{
    Percent = 0,
    AbsoluteValue = 10
}
public enum InteractionComparatorTarget
{
    Self = 0,
    TargetEnemy = 10
}
public enum Selector
{
    NoCondition = 0,
    ToTheLeftOf = 10,
    ToTheRightOf = 20,
    DealtDamage = 30,
    HasBuff = 50,
    DoesNotHaveBuff = 55,
    Stat = 60,
    IsGrounded = 70,
    HitGround = 80
}
[Serializable]
public struct InteractionCondition {

    public Selector selector;
    public InteractionComparator comparator;
    public InteractionCalculation calculation;
    public InteractionComparatorTarget target;
    public BuffNames buff;
    public Stat stat;
    public float threshold;

    public bool IsSatisified(GameUnit owner, GameUnit target, float damageDealt = 0, bool groundHit = false)
    {
        if (selector == Selector.NoCondition) { return true; }
        else if (selector == Selector.ToTheLeftOf && owner.body.position.x > target.body.position.x) { return false; }
        else if (selector == Selector.ToTheRightOf && owner.body.position.x < target.body.position.x) { return false; }
        else if (selector == Selector.DealtDamage)
        {
            return CheckComparator(damageDealt /
                (calculation == InteractionCalculation.Percent ? (target.stats.GetStat(Stat.Health) + target.stats.GetStat(Stat.Shield)) : 1));
        }
        else if (selector == Selector.Stat && calculation == InteractionCalculation.Percent)
        {
            return CheckComparator(((this.target == InteractionComparatorTarget.Self) ? owner : target).stats.GetValuePercentage(stat));
        }
        else if (selector == Selector.Stat && calculation == InteractionCalculation.AbsoluteValue)
        {
            return CheckComparator(((this.target == InteractionComparatorTarget.Self) ? owner : target).stats.GetCurrentValue(stat));
        }
        else if (selector == Selector.HasBuff || selector == Selector.DoesNotHaveBuff)
        {
            if (((this.target == InteractionComparatorTarget.Self) ? owner : target).buffHandler.buffs.Contains((int)buff))
            {
                bool active = ((this.target == InteractionComparatorTarget.Self) ? owner : target).buffHandler.buffs[(int)buff].isActive;
                return selector == Selector.HasBuff ? active : !active;
            }
            else
            {
                return selector == Selector.HasBuff ? false : true;
            }
        }
        else if (selector == Selector.IsGrounded)
        {
            return ((this.target == InteractionComparatorTarget.Self) ? owner : target).IsGrounded();
        }
        else if (selector == Selector.HitGround)
        {
            return groundHit;
        }

        return true;
    }

    private bool CheckComparator(float part)
    {
        if(comparator == InteractionComparator.Is)
        {
            return part * (calculation == InteractionCalculation.Percent ? 100 : 1) == threshold;

        }else if (comparator == InteractionComparator.Above)
        {
            return part * (calculation == InteractionCalculation.Percent ? 100 : 1) > threshold;

        }else if (comparator == InteractionComparator.Below)
        {
            return part * (calculation == InteractionCalculation.Percent ? 100 : 1) < threshold;
        }

        return false;
    }
}
