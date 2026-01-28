using Unity.Netcode;
using UnityEngine;

public class LocalVisibility : NetworkBehaviour
{
    [SerializeField] Renderer[] hideForOwner;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        foreach (var r in hideForOwner)
            r.enabled = false;
    }
}
