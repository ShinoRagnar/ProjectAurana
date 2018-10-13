using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Equipper
{
    Item Equip(Item i);
    void Unequip(Item i);
    void Materialize(Item i, Transform parent);
    GameUnit GetOwner();
    Item GetFirstItemOfType(System.Type type);
}
public class ItemEquipper : Equipper {

    public ListHash<Item> equipped = new ListHash<Item>();
    public GameUnit owner;

    public ItemEquipper(GameUnit ownerVal)
    {
        this.owner = ownerVal;
    }

    public Item Equip(Item i)
    {
        equipped.AddIfNotContains(i);
        i.AddEquipper(this);
        return i;
    }
    public void Unequip(Item i)
    {
        equipped.Remove(i);
        i.RemoveEquipper();
    }
    public void Materialize(Item i, Transform parent)
    {
        Global.instance.Materialize(i, parent);
    }
    public GameUnit GetOwner()
    {
        return owner;
    }
    public Item GetFirstItemOfType(System.Type type)
    {

        foreach (Item g in equipped)
        {
            if (g.GetType() == type)
            {
                return g;
            }
        }
        return null;
    }
	
}
