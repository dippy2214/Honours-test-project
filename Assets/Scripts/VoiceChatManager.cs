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
        // Unity lifecycle entry point — async logic delegated
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            SubscribeToAuthenticationEvents();

            // 1. Initialize Unity Services
            await UnityServices.InitializeAsync();

            // 2. Authenticate (anonymous is fine for test projects)
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            Debug.Log($"Authenticated as {AuthenticationService.Instance.PlayerId}");

            // 3. Initialize Vivox
            await VivoxService.Instance.InitializeAsync();

            // 4. Login to Vivox (this is REQUIRED)
            await VivoxService.Instance.LoginAsync();

            Debug.Log("Vivox login successful");
        }
        catch (Exception e)
        {
            Debug.LogError($"Vivox initialization failed: {e}");
        }
    }
    
    private void OnApplicationQuit()
    {
        UnsubscribeFromAuthenticationEvents();

        // Optional cleanup — safe, not strictly required for test projects
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