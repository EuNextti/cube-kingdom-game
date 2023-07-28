using UnityEngine;

public class PlayerSelectedItem : MonoBehaviour
{
    public Transform hand;

    private void Update()
    {
        transform.position = hand.position;
        transform.rotation = hand.rotation;
    }
}
