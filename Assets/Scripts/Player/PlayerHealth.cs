using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public bool isAlive = true;

    // Server-authoritative health
    public NetworkVariable<float> Health = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Modify health (server only)
    public void ModifyHealth(float amount)
    {
        if (!IsServer) return;

        Health.Value = Mathf.Clamp(Health.Value + amount, 0f, maxHealth);
        if (Health.Value == 0)
        {
            PlayerDeath();
        }
    }

    public void PlayerDeath()
    {
        if (!IsServer) return; // Only the server should notify

        Debug.Log($"Player {OwnerClientId} died!");
        isAlive = false;
        // Tell the GameManager that this player died
        GameManager.Instance.RegisterDeath(OwnerClientId);
    }
}