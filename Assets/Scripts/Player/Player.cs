using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Header("Player Scripts")]
    [SerializeField] private PlayerMove playerMove;
    [SerializeField] private PlayerHotkeys playerHotkeys;
    [SerializeField] private PlayerInventory playerInventory;

    [Header("Player Variables")]
    [SerializeField] private float movementSpeed;

    [Header("Player Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject UI;
    [SerializeField] private Transform hotkeys;

    private CinemachineFreeLook freeLookCamera;
    private Camera cam;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsLocalPlayer)
        {
            Destroy(UI);
            enabled = false;
            return;
        }

        name = "User" + "_" + NetworkManager.Singleton.LocalClientId;

        cam = FindObjectOfType<Camera>();
        freeLookCamera = FindObjectOfType<CinemachineFreeLook>();
        freeLookCamera.LookAt = transform;
        freeLookCamera.Follow = transform;
    }

    private void Update()
    {
        if (playerInventory.isOpen)
        {
            freeLookCamera.m_XAxis.m_MaxSpeed = 0;
        }
        else if (!playerInventory.isOpen)
        {
            freeLookCamera.m_XAxis.m_MaxSpeed = 400;
        }

        playerMove.Move(characterController, movementSpeed, cam.transform, animator);
        playerHotkeys.Play(hotkeys, playerInventory);
    }
}
