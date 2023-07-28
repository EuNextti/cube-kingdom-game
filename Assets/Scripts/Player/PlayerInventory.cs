using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : NetworkBehaviour
{
    [Header("Inventory Settings")]
    public List<InventoryObject> invItems = new();
    [SerializeField] private GameObject invPanel;
    [SerializeField] private Transform invSlots;
    [SerializeField] private Transform hotkeySlots;
    [SerializeField] private GameObject UI;
    [SerializeField] private GameObject invCanvasObject;
    [SerializeField] private KeyCode invButton = KeyCode.Tab;

    [Header("PickUp Settings")]
    [SerializeField] private LayerMask pickupLayer;
    [SerializeField] private float pickupDistance = 22;
    [SerializeField] private KeyCode pickupButton = KeyCode.E;

    private Camera cam;

    public bool isOpen = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsLocalPlayer)
        {
            Destroy(UI);
            enabled = false;
            return;
        }

        cam = FindObjectOfType<Camera>();

        invPanel.SetActive(isOpen);
    }

    private void Update()
    {
        if (Input.GetKeyDown(pickupButton)) Pickup();
        if (Input.GetKeyDown(invButton)) ToggleInventory();
    }

    private void Pickup()
    {
        Ray ray;
        ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100, pickupLayer))
        {
            if (hit.transform.GetComponent<GroundItem>() == null) return;

            pickupDistance = Vector3.Distance(transform.position, hit.transform.position);

            if (pickupDistance > 2) return;

            AddToInventory(hit.transform.GetComponent<GroundItem>().itemScriptable);
            DespawnObjectServerRpc(hit.transform.GetComponent<NetworkObject>().NetworkObjectId);
            UpdateInvUI();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnObjectServerRpc(ulong objectNetID)
    {
        NetworkObject netObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[objectNetID];
        Destroy(netObj);
        netObj.Despawn();
    }

    private void AddToInventory(Item newItem)
    {
        foreach (InventoryObject invItem in invItems)
        {
            if (invItem.item == newItem)
            {
                invItem.amount++;
                return;
            }
        }

        for (int i = 0; i < invSlots.childCount; i++)
        {
            Transform slot = invSlots.GetChild(i);

            if (slot.childCount == 0)
            {
                invItems.Add(new InventoryObject() { item = newItem, amount = 1, slot = slot });
                return;
            }
        }
    }

    private void ToggleInventory()
    {
        if (invPanel.activeSelf)
        {
            isOpen = false;
            invPanel.SetActive(isOpen);
        }
        else if (!invPanel.activeSelf)
        {
            UpdateInvUI();
            isOpen = true;
            invPanel.SetActive(isOpen);
        }
    }

    private void UpdateInvUI()
    {
        for (int i = 0; i < invSlots.childCount; i++)
        {
            foreach (Transform child in invSlots.GetChild(i))
            {
                Destroy(child.gameObject);
            }
        }

        for (int i = 0; i < hotkeySlots.childCount; i++)
        {
            foreach (Transform child in hotkeySlots.GetChild(i))
            {
                Destroy(child.gameObject);
            }
        }

        foreach (InventoryObject invItem in invItems)
        {
            GameObject go = Instantiate(invCanvasObject, invItem.slot);

            go.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = invItem.amount.ToString();

            go.transform.GetComponent<DragDrop>().itemName = invItem.item.itemName;
            go.transform.GetComponent<DragDrop>().inventoryUI = invPanel.transform;
            go.transform.GetComponent<DragDrop>().item = invItem.item;
            go.transform.GetComponent<DragDrop>().amount = invItem.amount;
            go.transform.GetComponent<DragDrop>().raycaster = UI.GetComponent<GraphicRaycaster>();

            go.transform.GetComponent<Image>().sprite = invItem.item.itemIcon;
        }
    }

    public void DropItem(Item item)
    {
        foreach (InventoryObject invItem in invItems)
        {
            if (invItem.item != item) continue;

            if (invItem.amount > 1)
            {
                invItem.amount--;
                DropItemServerRpc(invItem.item.itemName);
                UpdateInvUI();
                return;
            }

            if (invItem.amount <= 1)
            {
                invItems.Remove(invItem);
                DropItemServerRpc(invItem.item.itemName);
                UpdateInvUI();
                return;
            }
        }

    }

    [ServerRpc(RequireOwnership = false)]
    private void DropItemServerRpc(string objectID)
    {
        GameObject loaded = Resources.Load(objectID) as GameObject;
        GameObject go = Instantiate(loaded, new Vector3(transform.position.x, transform.position.y + 3, transform.position.z) + transform.forward, Quaternion.identity);
        go.GetComponent<NetworkObject>().Spawn();
    }

    [System.Serializable]
    public class InventoryObject
    {
        public Item item;
        public int amount;
        public Transform slot;
    }
}
