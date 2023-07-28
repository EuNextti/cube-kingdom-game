using Unity.Netcode.Components;
using UnityEngine;

public class ItemPositionAndRotation : MonoBehaviour
{
    private Transform rightHandBone;

    private void Start()
    {
        if (transform.parent != null)
        {
            GetComponent<NetworkTransform>().enabled = false;

            rightHandBone = transform.parent.Find("/" +
                transform.parent.name +
                "/Motion/B_Pelvis/B_Spine/B_Spine1" +
                "/B_Spine2/B_R_Clavicle/B_R_UpperArm" +
                "/B_R_Forearm/B_R_Hand").transform;
        }
    }

    private void Update()
    {
        UpdatePositionAndRotation(rightHandBone);
    }

    private void UpdatePositionAndRotation(Transform handTransform)
    {
        transform.SetPositionAndRotation(handTransform.position, handTransform.rotation);
    }
}

