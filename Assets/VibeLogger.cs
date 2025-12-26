using UnityEngine;
using System.Text;

public class VibeLogger : MonoBehaviour
{
    private StringBuilder logBuilder = new StringBuilder();
    private Vector2 scrollPosition;
    public bool showLog = true;

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Color code errors so you spot them instantly
        string color = type == LogType.Error || type == LogType.Exception ? "red" : "white";

        logBuilder.AppendLine($"<color={color}>[{type}] {logString}</color>");

        if (type == LogType.Exception)
        {
            logBuilder.AppendLine($"<color=red>{stackTrace}</color>");
        }
    }

    void OnGUI()
    {
        if (!showLog) return;

        // Create a background box
        GUI.Box(new Rect(10, 10, Screen.width - 20, Screen.height / 3), "Vibe Console");

        // Create a Copy Button
        if (GUI.Button(new Rect(Screen.width - 120, 15, 100, 30), "COPY ALL"))
        {
            GUIUtility.systemCopyBuffer = logBuilder.ToString(); // Copies to your clipboard!
            Debug.Log("Logs copied to clipboard!");
        }

        // Create a Clear Button
        if (GUI.Button(new Rect(Screen.width - 230, 15, 100, 30), "CLEAR"))
        {
            logBuilder.Clear();
        }

        // The Scrollable Text Area
        GUILayout.BeginArea(new Rect(20, 50, Screen.width - 40, (Screen.height / 3) - 50));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        // We use a Label instead of TextArea so Rich Text colors work
        GUILayout.Label(logBuilder.ToString());

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}