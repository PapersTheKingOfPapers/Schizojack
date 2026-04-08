using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class HostButtons : MonoBehaviour
{
    [SerializeField] Button m_StartHostButton;
    [SerializeField] Button m_StartClientButton;
  
    private void Awake()
    {
        if (!FindAnyObjectByType<EventSystem>())
        {
            var inputType = typeof(StandaloneInputModule);

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), inputType);
            eventSystem.transform.SetParent(transform);
        }
    }
    void Start()
    {
        m_StartHostButton.onClick.AddListener(StartHost);
        m_StartClientButton.onClick.AddListener(StartClient);
    }

    void StartClient()
    {
        NetworkManager.Singleton.StartClient();
       
        DeactivateButtons();
    }
    void StartHost()
    {
        NetworkManager.Singleton.StartHost();

        DeactivateButtons();
    }
    void DeactivateButtons()
    {
        m_StartHostButton.interactable = false;
        m_StartClientButton.interactable = false;
        this.gameObject.SetActive(false);
    }
}
