using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class ItemDescriptionReader : MonoBehaviour
{
    [Header("Text Mesh Pro")]
    [SerializeField] private TMP_Text Title;
    [SerializeField] private TMP_Text Description;
    [Header("Screen References")]
    [SerializeField] private RectTransform BottomRCorner;
    [SerializeField] private RectTransform TopRCorner;
    [SerializeField] private RectTransform BottomLCorner;
    [SerializeField] private RectTransform TopLCorner;
    [SerializeField] private InputActionAsset Assets;
    [Header("Settings")]
    [SerializeField] private float readDistance = 5f;

    private InputAction _look;
    private Vector2 look;

    private bool highlighted;
    void Update()
    {
        look = _look.ReadValue<Vector2>();

        BottomLCorner.gameObject.SetActive(highlighted);
        TopLCorner.gameObject.SetActive(highlighted);
        BottomRCorner.gameObject.SetActive(highlighted);
        TopRCorner.gameObject.SetActive(highlighted);

        Debug.DrawRay(transform.position, transform.forward * readDistance, Color.black);

        Ray ray = Camera.main.ScreenPointToRay(look);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, readDistance))
        {
            ItemDescription item = hit.collider.GetComponent<ItemDescription>();

            if (item != null)
            {
                highlighted = true;

                Title.text = item.itemTitle;
                Description.text = item.itemDescription;

                Renderer rend = hit.collider.GetComponent<Renderer>();
                if (rend != null)
                {
                    Bounds bounds = rend.bounds;

                    Vector3 min = bounds.min;
                    Vector3 max = bounds.max;

                    Vector3[] corners = new Vector3[8]
                    {
                        new Vector3(min.x, min.y, min.z),
                        new Vector3(max.x, min.y, min.z),
                        new Vector3(min.x, max.y, min.z),
                        new Vector3(max.x, max.y, min.z),
                        new Vector3(min.x, min.y, max.z),
                        new Vector3(max.x, min.y, max.z),
                        new Vector3(min.x, max.y, max.z),
                        new Vector3(max.x, max.y, max.z)
                    };

                    Vector3 minScreen = new Vector3(float.MaxValue, float.MaxValue, 0);
                    Vector3 maxScreen = new Vector3(float.MinValue, float.MinValue, 0);

                    foreach (Vector3 corner in corners)
                    {
                        Vector3 screenPoint = Camera.main.WorldToScreenPoint(corner);

                        minScreen = Vector3.Min(minScreen, screenPoint);
                        maxScreen = Vector3.Max(maxScreen, screenPoint);
                    }

                    TopLCorner.position = new Vector3(minScreen.x, maxScreen.y, 0);
                    TopRCorner.position = new Vector3(maxScreen.x, maxScreen.y, 0);
                    BottomLCorner.position = new Vector3(minScreen.x, minScreen.y, 0);
                    BottomRCorner.position = new Vector3(maxScreen.x, minScreen.y, 0);
                }

                return;
            }
        }

        Title.text = "";
        Description.text = "";
        highlighted = false;
    }
    private void OnEnable()
    {
        _look = Assets.FindAction("Player/Look");
        _look.Enable();
    }
    private void OnDisable()
    {
        if (_look != null)
            _look.Disable();
    }
}
