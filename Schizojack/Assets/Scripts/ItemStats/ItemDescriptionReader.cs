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

    private Camera playerCamera;
    private InputAction _look;
    private Vector2 look;

    void Update()
    {
        if (!IsOwner)
            return;

        look = _look.ReadValue<Vector2>();

        OverLay.gameObject.SetActive(false);

        Ray ray = playerCamera.ScreenPointToRay(look);

        if (Physics.Raycast(ray, out RaycastHit hit, readDistance))
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

                Renderer renderer = hit.collider.GetComponent<Renderer>();
                Bounds bounds = renderer.bounds;

                Vector3[] corners = new Vector3[8];

                corners[0] = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
                corners[1] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
                corners[2] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
                corners[3] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);

                corners[4] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
                corners[5] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
                corners[6] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
                corners[7] = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z);

                Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
                Vector2 max = new Vector2(float.MinValue, float.MinValue);

                foreach (Vector3 corner in corners)
                {
                    Vector3 screenPoint = Camera.main.WorldToScreenPoint(corner);

                    min = Vector2.Min(min, screenPoint);
                    max = Vector2.Max(max, screenPoint);
                }

                Vector2 size = max - min;
                Vector2 position = (max + min) / 2f;

                OverLay.position = position;
                OverLay.sizeDelta = size;

                return;
            }
        }

        //Resets text to nothing
        Title.text = "";
        Description.text = "";
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
