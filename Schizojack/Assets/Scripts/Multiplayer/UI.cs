using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.Collections;

public class UI : NetworkBehaviour
{
    [Header("Player Name UI")]
    [SerializeField] private List<TMP_Text> player1Name;
    [SerializeField] private List<TMP_Text> player2Name;
    [SerializeField] private List<TMP_Text> player3Name;
    [SerializeField] private List<TMP_Text> player4Name;

    [SerializeField] private List<GameObject> player1UI;
    [SerializeField] private List<GameObject> player2UI;
    [SerializeField] private List<GameObject> player3UI;
    [SerializeField] private List<GameObject> player4UI;

    [Header("Screen References")]
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject PlayMenu;
    [SerializeField] private GameObject HostMenu;
    [SerializeField] private GameObject JoinCodeMenu;
    [SerializeField] private GameObject JoinMenu;
    [SerializeField] private GameObject LoadingScreen;
    [Header("Other References")]
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_InputField codeInput;

    private Coroutine loadingTimeoutCoroutine;

    private NetworkVariable<FixedString64Bytes> player1 = new NetworkVariable<FixedString64Bytes>();
    private NetworkVariable<FixedString64Bytes> player2 = new NetworkVariable<FixedString64Bytes>();
    private NetworkVariable<FixedString64Bytes> player3 = new NetworkVariable<FixedString64Bytes>();
    private NetworkVariable<FixedString64Bytes> player4 = new NetworkVariable<FixedString64Bytes>();

    private Dictionary<ulong, int> clientToSlot = new();
    private Dictionary<int, ulong> slotToClient = new();

    private void OnPlayer1Changed(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        foreach (TMP_Text text in player1Name) text.text = newValue.ToString();
        UpdateSlotVisibility(1, newValue.ToString());
    }

    private void OnPlayer2Changed(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        foreach (TMP_Text text in player2Name) text.text = newValue.ToString();
        UpdateSlotVisibility(2, newValue.ToString());
    }

    private void OnPlayer3Changed(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        foreach (TMP_Text text in player3Name) text.text = newValue.ToString();
        UpdateSlotVisibility(3, newValue.ToString());
    }

    private void OnPlayer4Changed(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        foreach (TMP_Text text in player4Name) text.text = newValue.ToString();
        UpdateSlotVisibility(4, newValue.ToString());
    }
    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        if (clientId == NetworkManager.ServerClientId) return;

        int assignedSlot = -1;
        if (string.IsNullOrEmpty(player2.Value.ToString())) assignedSlot = 2;
        else if (string.IsNullOrEmpty(player3.Value.ToString())) assignedSlot = 3;
        else if (string.IsNullOrEmpty(player4.Value.ToString())) assignedSlot = 4;

        if (assignedSlot != -1)
        {
            clientToSlot[clientId] = assignedSlot;
            slotToClient[assignedSlot] = clientId;

            string randomName = GetRandomName();
            if (assignedSlot == 2) player2.Value = randomName;
            if (assignedSlot == 3) player3.Value = randomName;
            if (assignedSlot == 4) player4.Value = randomName;
        }
    }
    private void OnClientDisconnected(ulong clientId)
    {
        if (IsServer)
        {
            if (clientToSlot.TryGetValue(clientId, out int slot))
            {
                if (slot == 2) player2.Value = "";
                if (slot == 3) player3.Value = "";
                if (slot == 4) player4.Value = "";

                clientToSlot.Remove(clientId);
                slotToClient.Remove(slot);
            }
        }

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("MainMenu");
        }
    }
    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        player1.OnValueChanged += OnPlayer1Changed;
        player2.OnValueChanged += OnPlayer2Changed;
        player3.OnValueChanged += OnPlayer3Changed;
        player4.OnValueChanged += OnPlayer4Changed;

        if (IsServer)
        {
            player1.Value = "";
            player2.Value = "";
            player3.Value = "";
            player4.Value = "";

            player1.Value = GetRandomName();
            clientToSlot[NetworkManager.ServerClientId] = 1;
            slotToClient[1] = NetworkManager.ServerClientId;
        }

        RefreshUI();
    }
    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        player1.OnValueChanged -= OnPlayer1Changed;
        player2.OnValueChanged -= OnPlayer2Changed;
        player3.OnValueChanged -= OnPlayer3Changed;
        player4.OnValueChanged -= OnPlayer4Changed;
    }

    private void Kick(int slot)
    {
        if (!IsServer) return;

        if (slotToClient.TryGetValue(slot, out ulong clientId))
        {
            NetworkManager.Singleton.DisconnectClient(clientId);
        }
    }

    private List<string> firstNames = new List<string>()
    {
        "Charlie",
        "Steven",
        "Dave",
        "William",
        "Liam",
        "Richard",
        "Ballas",
        "Quinn",
        "Jason",
        "Weston",
        "Aaron",
        "Christian",
        "Matthew",
        "Copper",
        "Ethan",
        "Marcus",
        "Chuck",
        "Saul",
        "Walter",
        "Morgan",
        "Arthur",
    };
    private List<string> lastNames = new List<string>()
    {
        "Kirk",
        "Smith",
        "Afton",
        "Johnson",
        "Armstrong",
        "Miller",
        "Balling",
        "Brown",
        "Cockbox",
        "Goodman",
        "White",
        "Morgan",
        "Ford",
        "Freeman",
    };

    private enum menuStates
    {
        MainMenu,
        PlayMenu,
        HostMenu,
        JoinCodeMenu,
        JoinMenu,
        //Other Than Main Screens:
        Loading,
    }

    private menuStates menuState;
    private Stack<menuStates> history = new Stack<menuStates>();

    private void Awake()
    {
        menuState = menuStates.MainMenu;
        UpdateUI();
    }

    private void UpdateUI()
    {
        MainMenu.SetActive(false);
        PlayMenu.SetActive(false);
        HostMenu.SetActive(false);
        JoinCodeMenu.SetActive(false);
        JoinMenu.SetActive(false);
        LoadingScreen.SetActive(false);

        switch (menuState)
        {
            case menuStates.MainMenu:
                MainMenu.SetActive(true);
                break;
            case menuStates.PlayMenu:
                PlayMenu.SetActive(true);
                break;
            case menuStates.HostMenu:
                HostMenu.SetActive(true);
                break;
            case menuStates.JoinCodeMenu:
                JoinCodeMenu.SetActive(true);
                break;
            case menuStates.JoinMenu:
                JoinMenu.SetActive(true);
                break;
            case menuStates.Loading:
                LoadingScreen.SetActive(true);
                break;
        }
    }

    private void SetState(menuStates state, bool addToHistory = true)
    {
        if (menuState == state) return;

        if (menuState == menuStates.Loading && loadingTimeoutCoroutine != null)
        {
            StopCoroutine(loadingTimeoutCoroutine);
            loadingTimeoutCoroutine = null;
        }

        if (addToHistory)
            history.Push(menuState);

        menuState = state;
        UpdateUI();

        if (menuState == menuStates.Loading)
        {
            loadingTimeoutCoroutine = StartCoroutine(LoadingTimeout());
        }
    }

    public void onPlay()
    {
        SetState(menuStates.PlayMenu);
    }
    //JOINING
    public void onJoin()
    {
        SetState(menuStates.JoinCodeMenu);
    }
    public void onConfirmJoin()
    {

        JoinGame();
    }
    //QUITTING
    public void onBack()
    {
        while (history.Count > 0)
        {
            var previous = history.Pop();

            if (previous == menuStates.Loading)
                continue;

            menuState = previous;
            UpdateUI();
            return;
        }
    }
    public void onQuit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    //Kicking
    public void Kickplayer2() => Kick(2);

    public void Kickplayer3() => Kick(3);

    public void Kickplayer4() => Kick(4);

    private IEnumerator LoadingTimeout()
    {
        yield return new WaitForSeconds(20f);

        if (menuState == menuStates.Loading)
        {
            Debug.LogWarning("Loading timed out.");
            SetState(menuStates.MainMenu);
        }
    }
    /// <summary>
    /// HOSTING
    /// </summary>
    public void onStartServer()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        }
    }
    public void onQuitServer()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
    public async void HostGame()
    {
        SetState(menuStates.Loading, false);

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log("JOIN CODE: " + joinCode);

            if (codeText != null)
                codeText.text = "Code: " + joinCode;

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(allocation.ToRelayServerData("dtls"));

            NetworkManager.Singleton.StartHost();

            SetState(menuStates.HostMenu, false);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            SetState(menuStates.HostMenu);
        }
    }
    /// <summary>
    /// JOINING
    /// </summary>
    public async void JoinGame()
    {
        SetState(menuStates.Loading, false);

        try
        {
            string joinCode = codeInput.text;

            JoinAllocation joinAllocation =
                await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(joinAllocation.ToRelayServerData("dtls"));

            NetworkManager.Singleton.StartClient();

            Debug.Log("Joined with code: " + joinCode);

            SetState(menuStates.JoinMenu, false);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Join failed: " + e);
            SetState(menuStates.JoinCodeMenu);
        }
    }
    /// <summary>
    /// Names
    /// </summary>
    private string GetRandomName()
    {
        string first = firstNames[Random.Range(0, firstNames.Count)];

        string last = lastNames[Random.Range(0, lastNames.Count)];

        return first + " " + last;
    }

    private void RefreshUI()
    {
        string p1 = player1.Value.ToString();
        string p2 = player2.Value.ToString();
        string p3 = player3.Value.ToString();
        string p4 = player4.Value.ToString();

        foreach (var t in player1Name) t.text = p1;
        UpdateSlotVisibility(1, p1);

        foreach (var t in player2Name) t.text = p2;
        UpdateSlotVisibility(2, p2);

        foreach (var t in player3Name) t.text = p3;
        UpdateSlotVisibility(3, p3);

        foreach (var t in player4Name) t.text = p4;
        UpdateSlotVisibility(4, p4);
    }
    private void UpdateSlotVisibility(int slot, string playerName)
    {
        bool isActive = !string.IsNullOrEmpty(playerName);

        List<GameObject> uiList = slot switch
        {
            1 => player1UI,
            2 => player2UI,
            3 => player3UI,
            4 => player4UI,
            _ => null
        };

        if (uiList != null)
        {
            foreach (GameObject go in uiList)
            {
                go.SetActive(isActive);
            }
        }
    }
}
