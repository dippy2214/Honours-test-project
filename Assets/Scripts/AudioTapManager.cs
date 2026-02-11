using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Vivox;
using UnityEditor.PackageManager;

public class AudioTapManager : NetworkBehaviour
{
    public static AudioTapManager Instance { get; private set; }


    // Local dictionary for mapping Vivox IDs to player GameObjects
    private Dictionary<string, ulong> vivoxToClientID = new Dictionary<string, ulong>();
    private Dictionary<string, GameObject> participantTaps = new Dictionary<string, GameObject>();

    private void Awake()
    {
        // Setup singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    #region Registration

    // Call this after player logs into Vivox
    public void RegisterLocalVivox(string vivoxId)
    {
        // Inform all clients about this mapping
        RegisterVivoxServerRpc(NetworkManager.Singleton.LocalClientId, vivoxId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RegisterVivoxServerRpc(ulong clientId, string vivoxId)
    {
        vivoxToClientID[vivoxId] = clientId;
        // Broadcast to all clients
        RegisterVivoxClientRpc(clientId, vivoxId);
    }

    [ClientRpc]
    private void RegisterVivoxClientRpc(ulong clientId, string vivoxId)
    {
        if (IsServer) return;
        Debug.Log("registering player completed");
        vivoxToClientID[vivoxId] = clientId;
    }

    #endregion

    #region Vivox Participant Events

    private void OnParticipantAdded(VivoxParticipant participant)
    {
        Debug.Log($"Vivox participant joined: {participant.DisplayName}");
        if (participant.IsSelf)
            return;

        StartCoroutine(OnParticipantAddedIEnumerable(participant));
        
    }

    private System.Collections.IEnumerator OnParticipantAddedIEnumerable(VivoxParticipant participant)
    {
        GameObject tap = participant.CreateVivoxParticipantTap(participant.DisplayName + "_AudioTap");
        participantTaps[participant.PlayerId] = tap;

        // Parent to player if known
        while (!vivoxToClientID.TryGetValue(participant.PlayerId, out ulong Id))
        {
            Debug.Log($"couldnt find clientId matching playerId {participant.PlayerId}");
            yield return null;        
        }
        vivoxToClientID.TryGetValue(participant.PlayerId, out ulong clientId);
        tap.transform.SetParent(GetPlayerObjectByClientId(clientId).transform);
        tap.transform.localPosition = Vector3.zero;
        tap.transform.localRotation = Quaternion.identity;
        Debug.Log($"found tap {tap} for client {clientId}");
    }

    private void OnParticipantRemoved(VivoxParticipant participant)
    {
        if (participantTaps.TryGetValue(participant.PlayerId, out GameObject tap))
        {
            Destroy(tap);
            participantTaps.Remove(participant.PlayerId);
        }
    }

    #endregion

    private void OnDestroy()
    {
        if (VivoxService.Instance != null)
        {
            VivoxService.Instance.ParticipantAddedToChannel -= OnParticipantAdded;
            VivoxService.Instance.ParticipantRemovedFromChannel -= OnParticipantRemoved;
        }
    }

    #region Late Joiner Support

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;
        // Send all current Vivox mappings to the late joiner
        foreach (var kvp in vivoxToClientID)
        {
            SendVivoxMappingToLateJoinerClientRpc(kvp.Value, kvp.Key, clientId);
        }
    }

    [ClientRpc]
    private void SendVivoxMappingToLateJoinerClientRpc(ulong playerOwnerId, string vivoxId, ulong targetClientId)
    {
        if (IsServer) return;
        // Only the target client processes this mapping
        if (NetworkManager.LocalClientId != targetClientId) return;

        vivoxToClientID[vivoxId] = playerOwnerId;

    }
    #endregion

    public static NetworkObject GetPlayerObjectByClientId(ulong clientId)
    {
        foreach (var netObj in FindObjectsOfType<NetworkObject>())
        {
            if (netObj.IsPlayerObject && netObj.OwnerClientId == clientId)
                return netObj;
        }
        return null;
    }
}