using UnityEditor;
using UnityEngine;
using System.Reflection;
using Core.Framework.Network;
using System;

public class NetworkSimulatorEditor : EditorWindow
{
    private string eventName = "NetworkEvent.ON_GPT_RESPONSE";
    private string responseJson = "{\"content\": \"Simulated response\"}"; 
    private Vector2 scrollPos;

    [MenuItem("Tools/Network Simulator")]
    public static void ShowWindow()
    {
        GetWindow<NetworkSimulatorEditor>("Network Simulator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Network Response Simulator", EditorStyles.boldLabel);

        eventName = EditorGUILayout.TextField("Event Name", eventName);

        GUILayout.Label("Simulated Response JSON:");
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        responseJson = EditorGUILayout.TextArea(responseJson, GUILayout.Height(100));
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Simulate Response"))
        {
            SimulateResponse();
        }
    }

    private void SimulateResponse()
    {
        if (NetworkManager.Instance == null)
        {
            EditorUtility.DisplayDialog("Error", "NetworkManager instance not found!", "OK");
            return;
        }

        try
        {
            NetworkManager networkManager = NetworkManager.Instance;
            networkManager.TriggerEventTest<String>(eventName, responseJson);
            Debug.Log($"Simulated event '{eventName}' with response: {responseJson}");
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Simulation failed: {e.Message}", "OK");
            Debug.LogError($"Simulation Error: {e.StackTrace}");
        }
    }
}