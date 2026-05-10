using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNameTransferer : NetworkBehaviour
{
    public static PlayerNameTransferer Instance;

    public NetworkVariable<FixedString64Bytes> player1 = new NetworkVariable<FixedString64Bytes>();
    public NetworkVariable<FixedString64Bytes> player2 = new NetworkVariable<FixedString64Bytes>();
    public NetworkVariable<FixedString64Bytes> player3 = new NetworkVariable<FixedString64Bytes>();
    public NetworkVariable<FixedString64Bytes> player4 = new NetworkVariable<FixedString64Bytes>();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
