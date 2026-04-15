using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class ItemDescriptionReader : NetworkBehaviour
{
    [Header("Text Mesh Pro")]
    [SerializeField] private TMP_Text Title;
    [SerializeField] private TMP_Text Description;
    [Header("Screen References")]
    [SerializeField] private GameObject Hud; //parent for ui so it can be disabled for all other players so all wont see the same
    [SerializeField] private RectTransform OverLay;
    [SerializeField] private RectTransform InfoPanel;
    [Header("Input")]
    [SerializeField] private InputActionAsset Assets;
    [Header("Settings")]
    [SerializeField] private float readDistance = 5f;
    [SerializeField] private LayerMask detectLayer;

    private Camera playerCamera;
    private InputAction _look;
    private Vector2 look;

    void Update()
    {
        look = _look.ReadValue<Vector2>();

        OverLay.gameObject.SetActive(false);

        Ray ray = playerCamera.ScreenPointToRay(look);

        if (Physics.Raycast(ray, out RaycastHit hit, readDistance, detectLayer))
        {
            ItemDescription item = hit.collider.GetComponent<ItemDescription>();

            if (item != null)
            {
                //makes the overlay visible and sets the title and description text
                OverLay.gameObject.SetActive(true);

                Title.text = item.itemTitle;
                Description.text = item.itemDescription;

                //Updates the info panel height
                Description.ForceMeshUpdate();

                float textHeight = Description.GetRenderedValues(false).y;

                Vector2 infoSize = InfoPanel.sizeDelta;
                infoSize.y = textHeight + 30;
                InfoPanel.sizeDelta = infoSize;
                //ends here               

                //Box Scaling
                BoxCollider box = hit.collider.GetComponent<BoxCollider>();
                if (box != null)
                {
                    UpdateOverlayBounds(box, hit.collider.transform);
                }

                return;
            }
        }

        //Resets text to nothing
        Title.text = "";
        Description.text = "";
    }
    void UpdateOverlayBounds(BoxCollider box, Transform targetTransform)
    {
        Vector3 center = box.center;
        Vector3 size = box.size;

        Vector3[] corners = new Vector3[8];

        int i = 0;
        for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 localCorner = center + Vector3.Scale(size * 0.5f, new Vector3(x, y, z));
                    corners[i++] = targetTransform.TransformPoint(localCorner);
                }

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        foreach (Vector3 corner in corners)
        {
            Vector3 screenPoint = playerCamera.WorldToScreenPoint(corner);

            if (screenPoint.z < 0)
                continue;

            min = Vector2.Min(min, screenPoint);
            max = Vector2.Max(max, screenPoint);
        }

        OverLay.position = (min + max) * 0.5f;

        OverLay.sizeDelta = max - min;
    }

    public override void OnNetworkSpawn() //Basically On Enabled
    {
        if (!IsOwner)
        {
            Hud.SetActive(false);

            enabled = false;

            return;
        }

        playerCamera = GetComponentInChildren<Camera>();

        _look = Assets.FindAction("Player/Look");
        _look.Enable();
    }

    public override void OnNetworkDespawn() //Basically On Disabled
    {
        if (_look != null)
            _look.Disable();
    }
}
