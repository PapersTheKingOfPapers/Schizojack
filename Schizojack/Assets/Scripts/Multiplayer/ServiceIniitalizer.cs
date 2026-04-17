using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;

public class UnityInit : MonoBehaviour
{
    async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
}