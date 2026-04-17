using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Join : MonoBehaviour
{
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private GameObject ButtonsUi;
    [SerializeField] private GameObject lobbyUi;

    private bool joined = false;

    [SerializeField] private InputActionAsset Assets;
    private InputAction _join;
    private void OnEnable()
    {
        _join = Assets.FindAction("Player/Enter");
        _join.Enable();
    }
    private void OnDisable()
    {
        if (_join != null)
            _join.Disable();
    }

    public void Update()
    {
        if (_join.WasPressedThisFrame())
        {
            JoinGame();
        }

        ButtonsUi.SetActive(!joined);
        lobbyUi.SetActive(joined);
    }

    public async void JoinGame()
    {
        try
        {
            string joinCode = codeInput.text;

            JoinAllocation joinAllocation =
                await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.SetRelayServerData(joinAllocation.ToRelayServerData("dtls"));

            NetworkManager.Singleton.StartClient();

            joined = true;

            Debug.Log("Joined with code: " + joinCode);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Join failed: " + e);
        }
    }
    public void QuitGameClient()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        joined = false;
    }
}
