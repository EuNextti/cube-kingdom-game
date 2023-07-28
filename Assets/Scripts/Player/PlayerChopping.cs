using Unity.Netcode;
using UnityEngine;

public class PlayerChopping : NetworkBehaviour
{
    [SerializeField] private PlayerHotkeys playerHotkeys;
    [SerializeField] private Transform hitArea;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsLocalPlayer)
        {
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            OnHitServerRpc();
        }
    }


    [ServerRpc]
    private void OnHitServerRpc()
    {
        Vector3 colliderSize = Vector3.one * 0.3f;
        Collider[] colliders = Physics.OverlapBox(hitArea.position, colliderSize);

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<TreeDestruction>(out TreeDestruction treeDestruction))
            {
                if (treeDestruction.treeHealth <= 0)
                {
                    collider.transform.rotation = Quaternion.FromToRotation(collider.transform.position, transform.position);
                }
                else
                {
                    treeDestruction.treeHealth--;
                    Debug.Log("Tree Hitted");
                }
            }
        }
    }
}
