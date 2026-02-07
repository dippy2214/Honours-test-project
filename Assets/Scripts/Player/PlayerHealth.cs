using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public bool isAlive = true;
    public GameObject playerBodyParts;

    public NetworkVariable<float> Health = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public void ModifyHealth(float amount)
    {
        if (!IsServer) return;

        Health.Value = Mathf.Clamp(Health.Value + amount, 0f, maxHealth);

        if (Health.Value == maxHealth)
        {
            EnablePlayerBodyClientRpc();
        }

        if (Health.Value == 0)
        {
            PlayerDeath();
        }
    }

    public void PlayerDeath()
    {
        if (!IsServer) return; 

        Debug.Log($"Player {OwnerClientId} died!");
        isAlive = false;
        GameManager.Instance.RegisterDeath(OwnerClientId);
        DisablePlayerBodyClientRpc();
    }

    [ClientRpc]
    public void DisablePlayerBodyClientRpc()
    {
        playerBodyParts.SetActive(false);
        GetComponent<MeshRenderer>().enabled = false;
    }

    [ClientRpc]
    public void EnablePlayerBodyClientRpc()
    {
        playerBodyParts.SetActive(true);
        GetComponent<MeshRenderer>().enabled = true;
    }
}