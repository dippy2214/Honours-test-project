using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public Color blueTeamCol;
    public Color redTeamCol;
    public int playersPerTeam = 2;
    public float roundDelay = 5f;
    public List<GameObject> doors;

    public static GameManager Instance;
    public Dictionary<ulong, NetworkObject> players = new Dictionary<ulong, NetworkObject>();

    private void Awake() => Instance = this;

    public void RegisterPlayer(ulong clientId, NetworkObject player)
    {
        if (!IsServer) return;
        players[clientId] = player;
        SetPlayerSpectatorModeClientRpc(clientId, false);
        CheckAllPlayersJoined();
    }

    private void CheckAllPlayersJoined()
    {
        if (!IsServer) return;

        int connectedPlayers = NetworkManager.Singleton.ConnectedClients.Count;
        int registeredPlayers = players.Count;

        // Example: start match when all expected players have joined
        if (registeredPlayers == (playersPerTeam * 2))
        {
            Debug.Log("All players registered. " + players.Count + " Starting match!");
            StartRound();
        }
    }

    private void StartRound()
    {
        Debug.Log("Round starting!");

        // Open doors at round start
        foreach (var door in doors)
        {
            var anim = door.GetComponent<Animator>();
            if (anim != null)
                anim.SetBool("isOpen", true);
        }

        // Reset all players
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

            // Reset health
            health.ModifyHealth(100.0f);
            health.isAlive = true;

            // Teleport to spawn
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

            netTrans.Interpolate = false; // disable interpolation
            playerObj.transform.position = spawn.position;
            playerObj.transform.rotation = spawn.rotation;
            netTrans.Interpolate = true; // restore

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

            // Switch to player controls
            SetPlayerSpectatorModeClientRpc(clientId, false);
        }
    }

    public void RegisterDeath(ulong clientId)
    {
        if (!IsServer || !players.ContainsKey(clientId)) return;

        // Switch to spectator
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

        if (!teamAAlive) EndRound(Team.B);
        else if (!teamBAlive) EndRound(Team.A);
    }

    private void EndRound(Team winner)
    {
        Debug.Log($"Round over! {winner} wins.");

        // Close doors
        foreach (var door in doors)
        {
            var anim = door.GetComponent<Animator>();
            if (anim != null)
                anim.SetBool("isOpen", false);
        }

        if (IsServer)
            StartCoroutine(StartNextRoundAfterDelay());

        // Optionally, you could schedule the next round:
        // StartRound() after a short delay or via UI button
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

        // Reset root rotation
        playerObj.transform.rotation = Quaternion.Euler(rootEuler);

        // Reset camera pitch
        look.xRotation = 0f;
        look.cam.transform.localRotation = Quaternion.identity;

        // Reset spectator yaw/pitch
        specMotor.yaw = playerObj.transform.eulerAngles.y;
        specMotor.pitch = 0f;
    }

    [ClientRpc]
    void SetPlayerColorClientRpc(NetworkObjectReference playerRef, Color color)
    {
        if (playerRef.TryGet(out var playerObj))
        {
            var renderer = playerObj.GetComponentInChildren<MeshRenderer>();
            renderer.material.color = color;
        }
    }

    private System.Collections.IEnumerator StartNextRoundAfterDelay()
    {
        yield return new WaitForSeconds(roundDelay);

        // Optionally, you could reset team states, scores, or do a countdown UI here

        StartRound();
    }
}