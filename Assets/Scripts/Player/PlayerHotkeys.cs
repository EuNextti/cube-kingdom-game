using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHotkeys : NetworkBehaviour
{
    [SerializeField] private Transform hand;
    [SerializeField] private Animator animator;

    [Header("Player Hotkeys")]
    private readonly KeyCode[] slots = new KeyCode[] 
    { 
        KeyCode.Alpha1, 
        KeyCode.Alpha2, 
        KeyCode.Alpha3, 
        KeyCode.Alpha4, 
        KeyCode.Alpha5, 
        KeyCode.Alpha6, 
        KeyCode.Alpha7, 
        KeyCode.Alpha8, 
        KeyCode.Alpha9 
    };

    public int selectedSlot = 0;

    private Color32 selectedHotkey = new(255, 201, 121, 225);

    public GameObject selectedItem;

    public void Play(Transform hotkeys, PlayerInventory playerInventory)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (Input.GetKeyDown(slots[i]))
            {
                if (i > hotkeys.childCount - 1)
                {
                    UpdateSlotImage(hotkeys, 0);
                    return;
                }

                UpdateSlotImage(hotkeys, i);
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q) && hotkeys.GetChild(selectedSlot).childCount > 0)
        {
            GameObject child = hotkeys.GetChild(selectedSlot).GetChild(0).gameObject;

            playerInventory.DropItem(child.GetComponent<DragDrop>().item);

            if (child.GetComponent<DragDrop>().amount <= 1) Destroy(child);

            if (selectedItem != null)
            {
                DropItemRequestServerRpc(selectedItem.GetComponent<NetworkObject>().NetworkObjectId, child.GetComponent<DragDrop>().amount);
            }
        }
    }

    private void UpdateSlotImage(Transform hotkeys, int slot)
    {
        foreach (Transform child in hotkeys)
        {
            child.GetComponent<Image>().color = Color.white;
        }

        selectedSlot = slot;
        hotkeys.GetChild(selectedSlot).GetComponent<Image>().color = selectedHotkey;

        if (hotkeys.GetChild(selectedSlot).childCount > 0)
        {
            DragDrop dragDrop = hotkeys.GetChild(selectedSlot).GetChild(0).GetComponent<DragDrop>();

            HoldItemRequestServerRpc(OwnerClientId, dragDrop.itemName);
        }
    }

    [ServerRpc]
    private void HoldItemRequestServerRpc(ulong clientNetID, string itemName)
    {
        if (selectedItem != null) return;

        GameObject loaded = Resources.Load(itemName) as GameObject;
        GameObject go = Instantiate(loaded);
        go.GetComponent<NetworkObject>().SpawnWithOwnership(clientNetID);
        go.transform.SetParent(transform);

        SetGameObjectClientRpc(go.GetComponent<NetworkObject>().NetworkObjectId);
    }

    [ClientRpc]
    private void SetGameObjectClientRpc(ulong itemNetID)
    {
        NetworkObject spawnedItem = NetworkManager.Singleton.SpawnManager.SpawnedObjects[itemNetID];

        spawnedItem.transform.position = hand.position;
        spawnedItem.gameObject.AddComponent<PlayerSelectedItem>();
        spawnedItem.gameObject.GetComponent<PlayerSelectedItem>().hand = hand;

        GroundItem groundItem = spawnedItem.gameObject.GetComponent<GroundItem>();

        if (groundItem.itemScriptable.itemType == "Weapon")
        {
            animator.SetBool("holdingWeapon", true);
            spawnedItem.GetComponent<MeshCollider>().isTrigger = true;
            spawnedItem.tag = "Axe";
        }

        selectedItem = spawnedItem.gameObject;
    }

    [ServerRpc]
    private void DropItemRequestServerRpc(ulong itemNetID, int amount)
    {
        if (amount <= 1)
        {
            NetworkObject spawnedItem = NetworkManager.Singleton.SpawnManager.SpawnedObjects[itemNetID];

            GroundItem groundItem = spawnedItem.gameObject.GetComponent<GroundItem>();

            if (groundItem.itemScriptable.itemType == "Weapon")
            {
                ChangeAnimationClientRpc(false);
            }

            Destroy(selectedItem);
            Destroy(spawnedItem.gameObject);
            spawnedItem.Despawn();
        }
    }

    [ClientRpc]
    private void ChangeAnimationClientRpc(bool holdingWeapon)
    {
        animator.SetBool("holdingWeapon", holdingWeapon);
    }
}
