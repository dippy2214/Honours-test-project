using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerShootable : ShootableBase
{
    PlayerHealth healthComponent;
    public override void OnNetworkSpawn()
    {
        healthComponent = GetComponent<PlayerHealth>();
    }

    public override void OnShot(float damage, ulong shooterId)
    {
        healthComponent.ModifyHealth(-damage, shooterId);
        Debug.Log($"[{NetworkManager.Singleton.LocalClientId}] Took {damage} damage, remaining health: {healthComponent.Health.Value}");
    }
}
