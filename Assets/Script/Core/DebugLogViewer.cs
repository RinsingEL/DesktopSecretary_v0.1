//using UnityEditor;
//using UnityEngine;
//using Core.Framework.Utility;

//public class DebugLogViewer : EditorWindow
//{
//    private Vector2 scrollPos;
//    private bool autoRefresh = true;

//    [MenuItem("Tools/Debug Log Viewer")]
//    public static void ShowWindow()
//    {
//        GetWindow<DebugLogViewer>("Debug Log Viewer");
//    }

//    private void OnGUI()
//    {
//        GUILayout.Label("Debug Log Viewer", EditorStyles.boldLabel);

//        if (!EditorApplication.isPlaying)
//        {
//            GUILayout.Label("Please enter Play Mode to view logs.", EditorStyles.helpBox);
//            return;
//        }

//        // 只有运行时才访问DebugLogger
//        if (DebugLogger.Instance == null)
//        {
//            GUILayout.Label("DebugLogger not initialized in scene!", EditorStyles.helpBox);
//            return;
//        }

//        autoRefresh = EditorGUILayout.Toggle("Auto Refresh", autoRefresh);
//        if (GUILayout.Button("Refresh") || (autoRefresh && Event.current.type == EventType.Repaint))
//        {
//            Repaint();
//        }

//        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
//        var logs = DebugLogger.Instance.GetLogEntries();

//        foreach (var log in logs)
//        {
//            EditorGUILayout.LabelField($"[{log.Timestamp}] {log.Message}", EditorStyles.boldLabel);
//            foreach (var frame in log.StackTrace)
//            {
//                EditorGUILayout.LabelField($"  {frame.Method}");
//                EditorGUILayout.LabelField($"    File: {frame.FilePath}, Line: {frame.LineNumber}");
//            }
//            EditorGUILayout.Space();
//        }

//        EditorGUILayout.EndScrollView();

//        if (GUILayout.Button("Clear Logs"))
//        {
//            logs.Clear();
//            System.IO.File.WriteAllText(DebugLogger.Instance.GetComponent<DebugLogger>().logFilePath, "");
//        }
//    }

//    // 当窗口启用或Play Mode变化时更新状态
//    private void OnEnable()
//    {
//        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
//    }

//    private void OnDisable()
//    {
//        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
//    }

//    private void OnPlayModeStateChanged(PlayModeStateChange state)
//    {
//        Repaint(); // 状态变化时刷新窗口
//    }
//}