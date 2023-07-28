using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDrop : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public Image image;
    public string itemName;
    public int amount;
    public Transform inventoryUI;
    public Item item;
    public GraphicRaycaster raycaster;

    private PointerEventData pointerEventData;
    private EventSystem eventSystem;
    private PlayerInventory playerInventory;
    private Transform originalParent;

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(raycaster.transform);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        playerInventory = transform.root.GetComponent<PlayerInventory>();
        eventSystem = FindObjectOfType<EventSystem>();

        pointerEventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new();

        raycaster.Raycast(pointerEventData, results);

        if (results.Count > 0)
        {
            Transform slot = results[0].gameObject.transform;

            if (slot.CompareTag("InventorySlot"))
            {
                transform.SetParent(slot);
                image.raycastTarget = true;

                for (int i = 0; i < playerInventory.invItems.Count; i++)
                {
                    if (playerInventory.invItems[i].item.name == itemName)
                    {
                        playerInventory.invItems[i].slot = slot;
                        return;
                    }
                }
            }
            else
            {
                transform.SetParent(originalParent);
                image.raycastTarget = true;
            }
        }
        else
        {
            playerInventory = transform.root.GetComponent<PlayerInventory>();
            playerInventory.DropItem(item);
            Destroy(eventData.pointerDrag);
            return;
        }
    }
}
