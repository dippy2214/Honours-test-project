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
        if (VivoxService.Instance.ActiveChannels.ContainsKey("world_voice"))
        {   
            VivoxService.Instance.Set3DPosition(
                gameObject, 
                VivoxVoiceManager.worldProximityChannel
            );
        }
    }
}
