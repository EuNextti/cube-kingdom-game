using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    private PlayerInventory inventory;

    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount > 0) return;

        inventory = transform.root.GetComponent<PlayerInventory>();

        GameObject dropped = eventData.pointerDrag;
        DragDrop dragDrop = dropped.GetComponent<DragDrop>();

        for (int i = 0; i < inventory.invItems.Count; i++)
        {
            if (inventory.invItems[i].item.name == dragDrop.itemName)
            {
                inventory.invItems[i].slot = transform;
                return;
            }
        }
    }
}
