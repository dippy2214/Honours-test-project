using UnityEngine;
using System.IO;
using System;
using Unity.Services.Vivox;
using SteamAudio;

public class VivoxDebugLogger : MonoBehaviour
{
    private string logPath;

    void Awake()
    {
        // Saves to the same folder as the .exe
        logPath = Path.Combine(Application.dataPath, "Vivox_Debug_Log.txt");
        WriteToLog("--- New Session Started ---");
        WriteToLog($"OS: {SystemInfo.operatingSystem} | Time: {DateTime.Now}");
    }

    public void OnVivoxStatusChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Replace 'client' with your actual Vivox Client reference
        // This catches the exact state where it hangs (e.g., 'Connecting' vs 'LoggingIn')
        WriteToLog($"Vivox State Change: {e.PropertyName}");
    }

    public void HandleVivoxError(string context, Exception ex)
    {
        string errorMessage = $"[ERROR] {context}: {ex.Message}";

            errorMessage += $" | ErrorCode: {ex}";
        WriteToLog(errorMessage);
    }

    public void WriteToLog(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        string finalMessage = $"[{timestamp}] {message}";
        
        Debug.Log(finalMessage); // Still show in console

        try
        {
            using (StreamWriter sw = File.AppendText(logPath))
            {
                sw.WriteLine(finalMessage);
            }
        }
        catch (Exception) { /* Fail silently if file is locked */ }
    }
}
