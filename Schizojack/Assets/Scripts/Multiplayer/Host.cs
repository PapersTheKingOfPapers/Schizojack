using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Host : MonoBehaviour
{
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private GameObject ButtonsUi;
    [SerializeField] private GameObject HostUi;

    private bool hostMenuOpen = false;

    public void Update()
    {
        ButtonsUi.SetActive(!hostMenuOpen);
        HostUi.SetActive(hostMenuOpen);
    }

    public async void HostGame()
    {
        hostMenuOpen = true;

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        Debug.Log("JOIN CODE: " + joinCode);

        if (codeText != null)
            codeText.text = "Code: " + joinCode;

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(allocation.ToRelayServerData("dtls"));

        NetworkManager.Singleton.StartHost();
    }

    public void QuitGameHost()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        hostMenuOpen = false;
    }
}
