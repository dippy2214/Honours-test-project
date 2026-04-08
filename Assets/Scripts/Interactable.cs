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

    public void BaseInteract()
    {
        Debug.Log("Interacting");

        if (useEvents)
            GetComponent<InteractionEvent>().OnInteract.Invoke();

        InteractServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong interactingClientId = rpcParams.Receive.SenderClientId;

        NetworkObject playerObject =
            NetworkManager.Singleton.ConnectedClients[interactingClientId].PlayerObject;

        Interact(playerObject);

        var clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { interactingClientId }
            }
        };
        ApplyEffectClientRpc(clientRpcParams);
    }

    protected virtual void Interact(NetworkObject player)
    {
    }

    [ClientRpc]
    private void ApplyEffectClientRpc(ClientRpcParams clientRpcParams = default)
    {
        var playerUI = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerUI>();
        if (playerUI != null)
        {
            ApplyEffectToLocalUI(playerUI);
        }
    }

    protected virtual void ApplyEffectToLocalUI(PlayerUI playerUI)
    {
    }
}