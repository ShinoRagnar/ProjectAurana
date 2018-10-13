using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetPack : Item {

    Item leftBeam;
    Item rightBeam;

	public JetPack(string name, Item leftBeamItem, Item rightBeamItem) : base(name,leftBeamItem.prefab,null,null)
    {
        this.leftBeam = leftBeamItem;
        this.rightBeam = rightBeamItem;
        this.itemName = name;
        subItems.Add(rightBeam);
        subItems.Add(leftBeam);
    }

    public new void Show(Transform parent)
    {
        //Debug.Log("SHOWING");
        leftBeam.Show(parent);
        rightBeam.Show(parent);
        Disable();
    }
    public new JetPack Clone()
    {
        return new JetPack(this.itemName, leftBeam.Clone(), rightBeam.Clone());
    }
}
