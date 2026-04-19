using UnityEngine;

public class ItemDescription : MonoBehaviour
{
    [Header("Item Info")]
    public string itemTitle;

    [TextArea(5, 20)]
    public string itemDescription;

    [HideInInspector] public int trumpCardType;
}
