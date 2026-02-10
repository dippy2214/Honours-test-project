using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Vivox;

public sealed class VivoxSetup : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private async void Start()
    {
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            SubscribeToAuthenticationEvents();
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            Debug.Log($"Authenticated as {AuthenticationService.Instance.PlayerId}");

            await VivoxService.Instance.InitializeAsync();
            await VivoxService.Instance.LoginAsync();

            Debug.Log("Vivox login successful");
            AudioTapManager.Instance.RegisterLocalVivox(VivoxService.Instance.SignedInPlayerId);
        }
        catch (Exception e)
        {
            Debug.LogError($"Vivox initialization failed: {e}");
        }
    }
    
    private void OnApplicationQuit()
    {
        UnsubscribeFromAuthenticationEvents();

        if (VivoxService.Instance.IsLoggedIn)
        {
            _ = VivoxService.Instance.LogoutAsync();
        }
    }

    private void SubscribeToAuthenticationEvents()
    {
        AuthenticationService.Instance.SignedIn += OnPlayerSignedIn;
        AuthenticationService.Instance.SignedOut += OnPlayerSignedOut;
        AuthenticationService.Instance.SignInFailed += OnPlayerSignInFailed;
    }

    private void UnsubscribeFromAuthenticationEvents()
    {
        AuthenticationService.Instance.SignedIn -= OnPlayerSignedIn;
        AuthenticationService.Instance.SignedOut -= OnPlayerSignedOut;
        AuthenticationService.Instance.SignInFailed -= OnPlayerSignInFailed;
    }

    private void OnPlayerSignedIn()
    {
        Debug.Log($"Unity Authentication signed in: {AuthenticationService.Instance.PlayerId}");
    }

    private void OnPlayerSignedOut()
    {
        Debug.Log("Unity Authentication signed out");
    }

    private void OnPlayerSignInFailed(RequestFailedException request)
    {
        Debug.LogError($"Authentication failed: {request.Message}");
    }

    
}