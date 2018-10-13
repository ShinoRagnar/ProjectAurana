using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlight : MonoBehaviour {

    public Outline outline;
    public GameUnit owner;
    public MechItem item;

    private void OnMouseEnter()
    {
        outline.enabled = true;
        if(owner != null)
        {
            Global.Inventory.selectedGameUnits.AddIfNotContains(owner);
        }
        if(item != null)
        {
            Global.Inventory.selectedMechItems.AddIfNotContains(item);
            Global.Inventory.ShowTextOver(item);
        }
    }

    private void OnMouseExit()
    {
        Exit();
    }

    public void Exit()
    {
        outline.enabled = false;
        if (owner != null)
        {
            Global.Inventory.selectedGameUnits.Remove(owner);
        }
        if (item != null)
        {
            Global.Inventory.selectedMechItems.Remove(item);
            Global.Inventory.HideTextOver(item);
        }
    }
}
