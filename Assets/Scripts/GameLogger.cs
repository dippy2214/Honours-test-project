using System.IO;
using UnityEngine;
using Unity.Netcode;

public class GameLogger : MonoBehaviour
{
    private string logFilePath;

    private void Awake()
    {
        logFilePath = Path.Combine(
            Application.dataPath,
            $"match_log_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt"
        );

        WriteLine("Timestamp,Event,Details");
    }

    private void WriteLine(string line)
    {
        File.AppendAllText(logFilePath, line + "\n");
    }

    private string TimeStamp()
    {
        return Time.time.ToString("F2");
    }

    private bool CanLog()
    {
        return NetworkManager.Singleton != null &&
               NetworkManager.Singleton.IsServer;
    }

    public void LogRoundStart(int roundIndex)
    {
        if (!CanLog()) return;
        WriteLine($"{TimeStamp()},RoundStart,Round={roundIndex}");
    }

    public void LogRoundEnd(Team winner, int redWins, int blueWins)
    {
        if (!CanLog()) return;
        WriteLine($"{TimeStamp()},RoundEnd,Winner={winner},RedWins={redWins},BlueWins={blueWins}");
    }

    public void LogKill(ulong killerId, ulong victimId)
    {
        if (!CanLog()) return;
        WriteLine($"{TimeStamp()},Kill,Killer={killerId},Victim={victimId}");
    }

    public void LogPlayerJoined(ulong clientId)
    {
        if (!CanLog()) return;
        WriteLine($"{TimeStamp()},PlayerJoined,ClientId={clientId}");
    }

    public void LogPlayerDied(ulong clientId)
    {
        if (!CanLog()) return;
        WriteLine($"{TimeStamp()},PlayerDied,ClientId={clientId}");
    }
}
