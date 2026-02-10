using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public Color blueTeamCol;
    public Color redTeamCol;
    public int playersPerTeam = 2;
    public float roundDelay = 5f;
    private int roundIndex = 0;
    public List<GameObject> doors;
    public NetworkVariable<int> redTeamWins;
    public NetworkVariable<int> blueTeamWins;

    public static GameManager Instance;
    public Dictionary<ulong, NetworkObject> players = new Dictionary<ulong, NetworkObject>();

    private GameLogger logger;

    public void Start()
    {
        redTeamWins.Value = 0;
        blueTeamWins.Value = 0;
        logger = GetComponent<GameLogger>();
    }
    
    private void Awake() => Instance = this;

    public void RegisterPlayer(ulong clientId, NetworkObject player)
    {
        if (!IsServer) return;
        players[clientId] = player;
        logger?.LogPlayerJoined(clientId);
        AddPlayerToVoiceChatClientRpc(clientId);
        SetPlayerSpectatorModeClientRpc(clientId, false);
        CheckAllPlayersJoined();
    }

    private void CheckAllPlayersJoined()
    {
        if (!IsServer) return;

        int connectedPlayers = NetworkManager.Singleton.ConnectedClients.Count;
        int registeredPlayers = players.Count;

        if (registeredPlayers == (playersPerTeam * 2))
        {
            Debug.Log("All players registered. " + players.Count + " Starting match!");
            StartRound();
        }
    }

    private void StartRound()
    {
        roundIndex++;
        logger?.LogRoundStart(roundIndex);
        Debug.Log("Round starting!");

        RoundStartClientRPC();

        foreach (var door in doors)
        {
            var anim = door.GetComponent<Animator>();
            if (anim != null)
                anim.SetBool("isOpen", true);
        }

        var spawner = NetworkManager.Singleton.GetComponent<PlayerSpawner>();
        Transform[] teamASpawns = spawner.teamASpawns;
        Transform[] teamBSpawns = spawner.teamBSpawns;
        int teamASpawnCount = 0;
        int teamBSpawnCount = 0;
        foreach (var kvp in players)
        {
            ulong clientId = kvp.Key;
            NetworkObject playerObj = kvp.Value;

            PlayerTeam team = playerObj.GetComponent<PlayerTeam>();
            PlayerHealth health = playerObj.GetComponent<PlayerHealth>();
            SpectatorMotor specMotor = playerObj.GetComponent<SpectatorMotor>();
            InputManager input = playerObj.GetComponent<InputManager>();
            PlayerLook look = playerObj.GetComponent<PlayerLook>();
            Material material = playerObj.GetComponent<MeshRenderer>().material;

            health.ModifyHealth(100.0f, NetworkManager.LocalClientId);
            health.isAlive = true;

            Transform spawn;
            if (team.team == Team.A)
            {
                spawn = teamASpawns[teamASpawnCount];
                teamASpawnCount++;
                if (teamASpawnCount >= teamASpawns.Length-1)
                {
                    teamASpawnCount = 0;
                }
            }
            else
            {
                spawn = teamBSpawns[teamBSpawnCount];
                teamBSpawnCount++;
                if (teamBSpawnCount >= teamBSpawns.Length-1)
                {
                    teamBSpawnCount = 0;
                }
            }
           

            var netTrans = playerObj.gameObject.GetComponent<ClientNetworkTransform>();
            var cc = playerObj.GetComponent<CharacterController>();

            if (cc != null) cc.enabled = false;
            var motor = playerObj.GetComponent<PlayerMotor>();
            if (motor != null) motor.enabled = false;

            netTrans.Interpolate = false; 
            playerObj.transform.position = spawn.position;
            playerObj.transform.rotation = spawn.rotation;
            netTrans.Interpolate = true; 

            if (motor != null) motor.enabled = true;
            if (cc != null) cc.enabled = true;

            look.enabled = false;
            specMotor.enabled = false;
            // Reset camera and look
            look.xRotation = 0f;
            look.cam.transform.localRotation = Quaternion.identity;

            look.enabled = true;
            specMotor.enabled = true;

            Color col = (team.team == Team.A) ? redTeamCol : blueTeamCol;
            SetPlayerColorClientRpc(playerObj, col);

            ResetPlayerRotationClientRpc(clientId, spawn.rotation.eulerAngles);

            SetPlayerSpectatorModeClientRpc(clientId, false);
        }
    }

    public void RegisterDeath(ulong clientId, ulong killerId = ulong.MaxValue)
{
    if (!IsServer || !players.ContainsKey(clientId)) return;

    logger?.LogPlayerDied(clientId);

    if (killerId != ulong.MaxValue)
        logger?.LogKill(killerId, clientId);

    SetPlayerSpectatorModeClientRpc(clientId, true);

    players[clientId].GetComponent<PlayerHealth>().isAlive = false;
    CheckRoundEnd();
}

    private void CheckRoundEnd()
    {
        bool teamAAlive = false;
        bool teamBAlive = false;

        foreach (var p in players.Values)
        {
            if (!p.GetComponent<PlayerHealth>().isAlive) continue;
            if (p.GetComponent<PlayerTeam>().team == Team.A) teamAAlive = true;
            else teamBAlive = true;
        }

        if (!teamAAlive) 
        {
            blueTeamWins.Value += 1;
            EndRound(Team.B);
        }
        else if (!teamBAlive) 
        {
            redTeamWins.Value += 1;
            EndRound(Team.A);
        }


    }

    private void EndRound(Team winner)
    {
        logger?.LogRoundEnd(
            winner,
            redTeamWins.Value,
            blueTeamWins.Value
        );

        Debug.Log($"Round over! {winner} wins. total wins: " + (winner == Team.A ? redTeamWins.Value : blueTeamWins.Value));

        foreach (var door in doors)
        {
            var anim = door.GetComponent<Animator>();
            if (anim != null)
                anim.SetBool("isOpen", false);
        }

        RoundEndClientRPC(winner);

        if (IsServer)
            StartCoroutine(StartNextRoundAfterDelay());



    }

    [ClientRpc]
    private void SetPlayerSpectatorModeClientRpc(ulong clientId, bool spectating)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        var input = player.GetComponent<InputManager>();
        var specMotor = player.GetComponent<SpectatorMotor>();

        if (spectating)
        {
            input.EnableSpectatorControls();
            specMotor.SetSpectatorMode(true);
        }
        else
        {
            input.EnablePlayerControls();
            specMotor.SetSpectatorMode(false);
        }
    }

    [ClientRpc]
    private void ResetPlayerRotationClientRpc(ulong clientId, Vector3 rootEuler)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        var playerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        var look = playerObj.GetComponent<PlayerLook>();
        var specMotor = playerObj.GetComponent<SpectatorMotor>();

        playerObj.transform.rotation = Quaternion.Euler(rootEuler);

        look.xRotation = 0f;
        look.cam.transform.localRotation = Quaternion.identity;

        specMotor.yaw = playerObj.transform.eulerAngles.y;
        specMotor.pitch = 0f;
    }

    [ClientRpc]
    private void SetPlayerColorClientRpc(NetworkObjectReference playerRef, Color color)
    {
        if (playerRef.TryGet(out var playerObj))
        {
            var renderer = playerObj.GetComponentInChildren<MeshRenderer>();
            renderer.material.color = color;
        }
    }

    [ClientRpc]
    private void AddPlayerToVoiceChatClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        _ = VivoxVoiceManager.Instance.JoinVoiceChannelAsync();
    }

    [ClientRpc]
    private void RoundEndClientRPC(Team winner)
    {
        string text = (winner == Team.A ? "Round over. Red team wins!" : "Round over. Blue team wins!");
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerUI>().RoundEndTextEnable(text);
    }

    [ClientRpc]
    private void RoundStartClientRPC()
    {
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerUI>().RoundEndTextDisable();
    }

    private System.Collections.IEnumerator StartNextRoundAfterDelay()
    {
        yield return new WaitForSeconds(roundDelay);

        StartRound();
    }
}