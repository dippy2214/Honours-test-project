using UnityEngine;
using Unity.Netcode;

public abstract class ShootableBase : NetworkBehaviour
{
    public abstract void OnShot(float damage, ulong shooterId);

    [ClientRpc]
    public void OnShotClientRpc(float damage, ClientRpcParams rpcParams = default)
    {
        //OnShot(damage);
    }
}