using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class Interactable : NetworkBehaviour
{
    public bool useEvents;
    public string promptMessage;

    public virtual string OnLook()
    {
        return promptMessage;
    }

    // Called by any client when interacting
    public void BaseInteract()
    {
        Debug.Log("Interacting");

        if (useEvents)
            GetComponent<InteractionEvent>().OnInteract.Invoke();

        // Tell server to handle interaction
        InteractServerRpc();
    }

    // Runs on the server
    [ServerRpc(RequireOwnership = false)]
    private void InteractServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong interactingClientId = rpcParams.Receive.SenderClientId;

        NetworkObject playerObject =
            NetworkManager.Singleton.ConnectedClients[interactingClientId].PlayerObject;

        // Apply server-side logic (gameplay effects)
        Interact(playerObject);

        // Tell owning client to update their UI
        var clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { interactingClientId }
            }
        };
        ApplyEffectClientRpc(clientRpcParams);
    }

    // Override in subclasses for server-side logic
    protected virtual void Interact(NetworkObject player)
    {
        // Example: apply damage server-side if needed
    }

    // Runs only on the owning client
    [ClientRpc]
    private void ApplyEffectClientRpc(ClientRpcParams clientRpcParams = default)
    {
        var playerUI = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerUI>();
        if (playerUI != null)
        {
            ApplyEffectToLocalUI(playerUI);
        }
    }

    // Override in subclasses for client-local UI updates
    protected virtual void ApplyEffectToLocalUI(PlayerUI playerUI)
    {
        // Example: playerUI.AddHealth(-10);
    }
}