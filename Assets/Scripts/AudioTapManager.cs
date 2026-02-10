using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Vivox;

public class AudioTapManager : NetworkBehaviour
{
    // Singleton instance
    public static AudioTapManager Instance { get; private set; }

    // Maps VivoxID -> Player GameObject
    private Dictionary<string, GameObject> vivoxToPlayerGO = new Dictionary<string, GameObject>();

    // Maps VivoxID -> AudioTap GameObject
    private Dictionary<string, GameObject> participantTaps = new Dictionary<string, GameObject>();

    // VivoxID -> AudioTap GameObject waiting for player GameObject
    private Dictionary<string, GameObject> pendingTaps = new Dictionary<string, GameObject>();

    private void Awake()
    {
        // Singleton setup
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
        // Subscribe to Vivox events
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;

        // Track late joiners
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void Update()
    {
        if (pendingTaps.Count == 0) return;

        // Try parenting any pending taps
        List<string> readyKeys = new List<string>();
        foreach (var kvp in pendingTaps)
        {
            if (TryParentTap(kvp.Key, kvp.Value))
                readyKeys.Add(kvp.Key);
        }

        foreach (string key in readyKeys)
            pendingTaps.Remove(key);
    }

    #region Registration

    // Call this after the local player logs into Vivox
    public void RegisterLocalVivox(string vivoxId)
    {
        // Send mapping to server
        RegisterVivoxServerRpc(NetworkManager.LocalClientId, vivoxId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RegisterVivoxServerRpc(ulong clientId, string vivoxId)
    {
        // Broadcast mapping to all clients
        RegisterVivoxClientRpc(clientId, vivoxId);
    }

    [ClientRpc]
    private void RegisterVivoxClientRpc(ulong clientId, string vivoxId)
    {
        // Try to find player GameObject
        var playerNetObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
        if (playerNetObj != null)
        {
            vivoxToPlayerGO[vivoxId] = playerNetObj.gameObject;

            // If we have a pending tap, parent it now
            if (pendingTaps.TryGetValue(vivoxId, out GameObject tap))
            {
                TryParentTap(vivoxId, tap);
                pendingTaps.Remove(vivoxId);
            }
        }
        else
        {
            Debug.LogWarning($"Player NetworkObject not ready for ClientId {clientId}, VivoxID {vivoxId}");
        }
    }

    #endregion

    #region Vivox Participant Events

    private void OnParticipantAdded(VivoxParticipant participant)
    {
        Debug.Log($"Vivox participant joined: {participant.DisplayName}");

        if (participant.IsSelf)
            return;

        // Create audio tap
        GameObject tap = participant.CreateVivoxParticipantTap(participant.DisplayName + "_AudioTap");
        participantTaps[participant.PlayerId] = tap;

        // Try parenting now, otherwise queue
        if (!TryParentTap(participant.PlayerId, tap))
        {
            pendingTaps[participant.PlayerId] = tap;
            Debug.Log($"Queuing tap for Vivox ID {participant.PlayerId}, player object not ready yet");
        }
    }

    private void OnParticipantRemoved(VivoxParticipant participant)
    {
        if (participantTaps.TryGetValue(participant.PlayerId, out GameObject tap))
        {
            Destroy(tap);
            participantTaps.Remove(participant.PlayerId);
        }

        if (pendingTaps.ContainsKey(participant.PlayerId))
            pendingTaps.Remove(participant.PlayerId);
    }

    #endregion

    #region Late Joiner Support

    private void OnClientConnected(ulong clientId)
    {
        // Send all current Vivox mappings to the late joiner
        foreach (var kvp in vivoxToPlayerGO)
        {
            if (kvp.Value.TryGetComponent<NetworkObject>(out NetworkObject netObj))
            {
                SendVivoxMappingToLateJoinerClientRpc(netObj.OwnerClientId, kvp.Key, clientId);
            }
        }
    }

    [ClientRpc]
    private void SendVivoxMappingToLateJoinerClientRpc(ulong playerOwnerId, string vivoxId, ulong targetClientId)
    {
        // Only the late joiner processes this
        if (NetworkManager.LocalClientId != targetClientId) return;

        var playerNetObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerOwnerId);
        if (playerNetObj != null)
        {
            vivoxToPlayerGO[vivoxId] = playerNetObj.gameObject;

            // Parent any pending tap
            if (pendingTaps.TryGetValue(vivoxId, out GameObject tap))
            {
                TryParentTap(vivoxId, tap);
                pendingTaps.Remove(vivoxId);
            }
        }
        else
        {
            Debug.LogWarning($"Late joiner: Player object not ready for OwnerId {playerOwnerId}, VivoxID {vivoxId}");
        }
    }

    #endregion

    #region Helper

    private bool TryParentTap(string vivoxId, GameObject tap)
    {
        if (vivoxToPlayerGO.TryGetValue(vivoxId, out GameObject playerGO))
        {
            tap.transform.SetParent(playerGO.transform);
            tap.transform.localPosition = Vector3.zero;
            tap.transform.localRotation = Quaternion.identity;
            return true;
        }
        return false;
    }

    #endregion

    private void OnDestroy()
    {
        if (VivoxService.Instance != null)
        {
            VivoxService.Instance.ParticipantAddedToChannel -= OnParticipantAdded;
            VivoxService.Instance.ParticipantRemovedFromChannel -= OnParticipantRemoved;
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}