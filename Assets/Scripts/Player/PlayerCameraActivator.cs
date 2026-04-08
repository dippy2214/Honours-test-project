using Unity.Netcode;
using UnityEngine;

public class PlayerCameraActivator : NetworkBehaviour
{
    [SerializeField]
    private Camera playerCamera;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(true);
        }
    }
}
