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

    // Local client-side logic
    public override void OnShot(float damage)
    {
        healthComponent.ModifyHealth(-damage);
        Debug.Log($"[{NetworkManager.Singleton.LocalClientId}] Took {damage} damage, remaining health: {healthComponent.Health.Value}");
    }
}
