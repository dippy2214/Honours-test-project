using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    public Canvas canvas;
    public Button hostButton;
    public Button clientButton;
    public TMP_InputField ipInput;
    public Camera menuCamera;

    private void Awake()
    {
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("Started Host");
            OnStartNetworkSession();
        }
        else
        {
            Debug.LogError("Failed to start host!");
        }
    }

    private void StartClient()
    {

        // Use IP from input field or localhost
        string ip = string.IsNullOrEmpty(ipInput.text) ? "127.0.0.1" : ipInput.text;
        NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>().ConnectionData.Address = ip;
        NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>().ConnectionData.Port = 7777;
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log($"Started Client connecting to {ip}");
            OnStartNetworkSession();
        }
        else
        {
            Debug.LogError($"Failed to start client connecting to {ip}!");
        }
    }

    private void OnStartNetworkSession()
    {
        // Hide the UI
        if (canvas != null)
            canvas.gameObject.SetActive(false);

        // Lock and hide the cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (menuCamera != null)
            menuCamera.gameObject.SetActive(false);
    }
}
