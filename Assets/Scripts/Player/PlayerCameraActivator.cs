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
            // enable this player's camera only for the owning client
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(true);
        }
    }
}
