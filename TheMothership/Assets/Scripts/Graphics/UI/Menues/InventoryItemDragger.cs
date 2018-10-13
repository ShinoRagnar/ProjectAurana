using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
/*
public class InventoryItemDragger : MonoBehaviour, 
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private InventoryHandler inventoryHandler;
    private MechItem mechItem;
    private MechItemClass mechClass;

    public void SetItem(InventoryHandler ih, MechItem mi)
    {
        mechClass = MechItem.GetClass(mi);
        inventoryHandler = ih;
        mechItem = mi;
    }

    public void OnBeginDrag(PointerEventData data)
    {
        inventoryHandler.OnBeginDrag(mechItem, mechItem.owner.mech.IsEquipped(mechItem) ? InventoryType.Equipment : InventoryType.Inventory, mechClass);
    }
    public void OnDrag(PointerEventData data)
    {
        inventoryHandler.OnDrag(mechItem, mechItem.owner.mech.IsEquipped(mechItem) ? InventoryType.Equipment : InventoryType.Inventory, mechClass);
    }
    public void OnEndDrag(PointerEventData data)
    {
        inventoryHandler.OnEndDrag(mechItem, mechItem.owner.mech.IsEquipped(mechItem) ? InventoryType.Equipment : InventoryType.Inventory, mechClass);
    }
    public void OnPointerClick(PointerEventData data)
    {
        inventoryHandler.OnPointerClick(mechItem, mechItem.owner.mech.IsEquipped(mechItem) ? InventoryType.Equipment : InventoryType.Inventory, mechClass);
    }

    public void OnPointerEnter(PointerEventData data)
    {
        inventoryHandler.OnPointerEnter(mechItem, mechItem.owner.mech.IsEquipped(mechItem) ? InventoryType.Equipment : InventoryType.Inventory, mechClass);
    }
    public void OnPointerExit(PointerEventData data)
    {
        inventoryHandler.OnPointerExit(mechItem, mechItem.owner.mech.IsEquipped(mechItem) ? InventoryType.Equipment : InventoryType.Inventory, mechClass);
    }
}*/
