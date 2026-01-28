using UnityEngine;
using Unity.Netcode;

public abstract class ShootableBase : NetworkBehaviour
{
    // Local client logic when hit
    public abstract void OnShot(float damage);

    // ClientRpc to tell the owning client to run OnShot
    [ClientRpc]
    public void OnShotClientRpc(float damage, ClientRpcParams rpcParams = default)
    {
        //OnShot(damage);
    }
}