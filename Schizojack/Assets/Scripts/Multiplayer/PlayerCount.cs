using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerCount : MonoBehaviour
{
    [SerializeField] private TMP_Text playerCountText;
    void Update()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            int count = NetworkManager.Singleton.ConnectedClientsList.Count;
            playerCountText.text = $"Players: {count}";
        }
        else
        {
            playerCountText.text = "Players: 0";
        }
    }
}
