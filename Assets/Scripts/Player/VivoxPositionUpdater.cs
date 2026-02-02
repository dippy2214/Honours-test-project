using UnityEngine;
using Unity.Services.Vivox;
using Unity.Netcode;

public class VivoxProximityUpdater : MonoBehaviour
{
    void Update()
    {
        if (!GetComponent<NetworkObject>().IsOwner)
        {
            return;
        }
        VivoxService.Instance.Set3DPosition(
            gameObject, 
            VivoxVoiceManager.worldProximityChannel
        );
    }
}
