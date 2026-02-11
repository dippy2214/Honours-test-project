using Unity.Netcode;
using UnityEngine;
using Unity.Services.Vivox;

public class PlayerSpawner : MonoBehaviour
{
    public GameManager gameManager;
    public NetworkObject playerPrefab;

    public Transform[] teamASpawns;
    public Transform[] teamBSpawns;

    private int teamACount;
    private int teamBCount;

    private void Start()
    {
        Debug.Log("game manager enabled");
        if (NetworkManager.Singleton != null)
        {
            Debug.Log("network manager found");
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        //Debug.Log("gameManager client connection detected");
        if (!NetworkManager.Singleton.IsServer)
            return;

        bool goTeamA = teamACount <= teamBCount;

        Transform spawn = goTeamA ?
            teamASpawns[teamACount % teamASpawns.Length] :
            teamBSpawns[teamBCount % teamBSpawns.Length];

        if (goTeamA) teamACount++; else teamBCount++;

        var player = Instantiate(playerPrefab, spawn.position, spawn.rotation);
        player.SpawnAsPlayerObject(clientId);
        player.GetComponent<PlayerTeam>().team = goTeamA ? Team.A : Team.B;

        gameManager.RegisterPlayer(clientId, player.GetComponent<NetworkObject>());
        RegisterPlayerWithAudioTapManagerClientRpc(clientId);
    }

    [ClientRpc]
    private void RegisterPlayerWithAudioTapManagerClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        AudioTapManager.Instance.RegisterLocalVivox(VivoxService.Instance.SignedInPlayerId);
    }


}