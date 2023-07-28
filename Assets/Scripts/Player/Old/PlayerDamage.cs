using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDamage : NetworkBehaviour
{
    public NetworkVariable<int> health = new(10);

    [SerializeField] private Animator animator;
    [SerializeField] private Slider slider;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent<NetworkObject>(out var enemyId))
        {
            if (other.CompareTag("Player") && enemyId.OwnerClientId != OwnerClientId)
            {
                if (IsServer && IsLocalPlayer)
                {
                    DealDamage(enemyId.OwnerClientId, 1);
                }
                else if (IsClient && IsLocalPlayer)
                {
                    SubmitDealDamageRequestServerRpc(enemyId.OwnerClientId, 1);
                }
            }
        }
    }

    private void Update()
    {
        slider.value = health.Value;
    }

    public void DealDamage(ulong enemy, int damage)
    {
        var player = NetworkManager.Singleton.ConnectedClients[enemy].PlayerObject;

        if (player != null)
        {
            player.GetComponent<PlayerDamage>().TakeDamage(damage);
        }
    }

    public void TakeDamage(int damage)
    {
        health.Value -= damage;
        animator.SetTrigger("hit");

        Debug.Log(health);
    }

    [ServerRpc]
    public void SubmitDealDamageRequestServerRpc(ulong enemy, int damage)
    {
        DealDamage(enemy, damage);
    }
}
