using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Logger : MonoBehaviour
{
    [Header("Logging Settings")]
    public float logInterval = 1.0f;   // Seconds between log entries
    public bool logToConsole = true;

    private float timer;
    private int frameCount;
    private string logFilePath;

    void Awake()
    {
        // Safe location for builds
        logFilePath = Path.Combine(Application.dataPath, "fps_log.txt");

        // Write header once
        if (!File.Exists(logFilePath))
        {
            File.WriteAllText(logFilePath, "Time(s),FPS\n");
        }
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;
        frameCount++;

        if (timer >= logInterval)
        {
            float fps = frameCount / timer;
            float timeStamp = Time.time;

            string line = $"{timeStamp:F2},{fps:F2}\n";
            File.AppendAllText(logFilePath, line);

            timer = 0f;
            frameCount = 0;
        }
    }
}
