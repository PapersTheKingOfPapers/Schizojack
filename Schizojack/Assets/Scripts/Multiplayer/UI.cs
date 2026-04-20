using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool IsMainMenu = true;
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
}
