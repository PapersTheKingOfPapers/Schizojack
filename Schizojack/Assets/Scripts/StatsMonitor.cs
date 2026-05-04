using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StatsMonitor : MonoBehaviour
{
    [SerializeField] private GameObject statsUI;
    [SerializeField] private Button statsButton;
    [SerializeField] private GameObject optionsUI;
    [SerializeField] private Button optionsButton;
    [SerializeField] private GameObject areYouSureUI;

    private enum menuStates
    {
        stats,
        options,
        areYouSure
    }
    private menuStates menuState;
    private void Awake()
    {
        menuState = menuStates.stats;
        UpdateUI();
    }

    private void Update()
    {
        //The fucking heartrate or sumshit in here.
    }

    //Buttons//
    public void stats() //Stats Menu
    {
        menuState = menuStates.stats;
        UpdateUI();
    }
    public void options() //Options Menu
    {
        menuState = menuStates.options;
        UpdateUI();
    }
    public void back() //Back
    {
        menuState = menuStates.options;
        UpdateUI();
    }
    public void quit()
    {
        menuState = menuStates.areYouSure;
        UpdateUI();
    }
    public void confirmQuit() //Suicide
    {
        //Suicide code here When pressed player dies.
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        SceneManager.LoadScene("MainMenu");
    }

    private void UpdateUI()
    {
        statsUI.SetActive(false);
        optionsUI.SetActive(false);
        areYouSureUI.SetActive(false);

        statsButton.interactable = true;
        optionsButton.interactable = true;

        switch (menuState)
        {
            case menuStates.stats:
                statsUI.SetActive(true);
                statsButton.interactable = false;
                break;
            case menuStates.options:
                optionsUI.SetActive(true);
                optionsButton.interactable = false;
                break;
            case menuStates.areYouSure:
                areYouSureUI.SetActive(true);
                break;
        }
    }
}
