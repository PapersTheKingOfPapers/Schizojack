using UnityEngine;
using UnityEngine.UI;

public class DisableButton : MonoBehaviour
{
    private Button buton;
    private void Awake()
    {
        buton = GetComponent<Button>();

        buton.interactable = false;
    }
}
