using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerCount : MonoBehaviour
{
    [SerializeField] private TMP_Text[] playerCountText;
    void Update()
    {
        string text;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            int count = NetworkManager.Singleton.ConnectedClientsList.Count;
            text = $"Players: {count}";
        }
        else
        {
            text = "Players: 0";
        }

        foreach (var tmp in playerCountText)
        {
            if (tmp != null)
                tmp.text = text;
        }
    }
}
