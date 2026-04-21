using UnityEngine;
using UnityEngine.UI;

public class ScanLines : MonoBehaviour
{
    [SerializeField] private float speed = 0.2f;
    private RawImage img;
    void Start()
    {
        img = GetComponent<RawImage>();
    }
    void Update()
    {
        Rect uv = img.uvRect;
        uv.y += Time.deltaTime * speed;

        if (uv.y > 100f)
        {
            uv.y = 0f;
        }

        img.uvRect = uv;
    }
}
