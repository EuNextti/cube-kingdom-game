using UnityEngine;

[CreateAssetMenu(menuName = "Items/New Item", fileName = "New Item", order = 0)]
public class SO_Item : ScriptableObject
{
    public GameObject item;
    public string itemName;
    public float itemDamage;
    public AnimatorOverrideController overrideController;
}
