using TMPro;
using UnityEngine;

public class ItemDescriptionReader : MonoBehaviour
{
    [Header("Text Mesh Pro")]
    [SerializeField] private TMP_Text Title;
    [SerializeField] private TMP_Text Description;

    [Header("Settings")]
    [SerializeField] private float readDistance = 5f;
    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * readDistance, Color.black);

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, readDistance))
        {
            ItemDescription item = hit.collider.GetComponent<ItemDescription>();

            if (item != null)
            {
                Title.text = item.itemTitle;
                Description.text = item.itemDescription;
                return;
            }
        }

        Title.text = "";
        Description.text = "";
    }
}
