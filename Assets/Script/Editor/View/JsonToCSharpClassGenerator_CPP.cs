using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;

public class JsonToCSharpClassGenerator_CPP : EditorWindow
{
    private string jsonText = "";
    private string className = "GeneratedClass";
    private Vector2 scrollPos;
    private int testIterations = 100;

    [MenuItem("Tools/JSON to C# Class Generator (CPP)")]
    public static void ShowWindow()
    {
        GetWindow<JsonToCSharpClassGenerator_CPP>("JSON to C# Class Generator (CPP)");
    }

    private void OnGUI()
    {
        GUILayout.Label("JSON to C# Class Generator (CPP)", EditorStyles.boldLabel);

        className = EditorGUILayout.TextField("Class Name", className);
        testIterations = EditorGUILayout.IntField("Test Iterations", testIterations);

        GUILayout.Label("Input JSON:");
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        jsonText = EditorGUILayout.TextArea(jsonText, GUILayout.Height(200));
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Generate C# Class"))
        {
            GenerateClassFromJson();
        }

        if (GUILayout.Button("Test Performance"))
        {
            TestPerformance();
        }
    }

    [DllImport("JsonToCSharp")]
    private static extern System.IntPtr GenerateClassFromJson(string jsonText, string className);

    [DllImport("JsonToCSharp")]
    private static extern void FreeString(System.IntPtr str);

    private void GenerateClassFromJson()
    {
        if (string.IsNullOrEmpty(jsonText))
        {
            EditorUtility.DisplayDialog("Error", "请先输入JSON", "OK");
            return;
        }

        System.IntPtr resultPtr = GenerateClassFromJson(jsonText, className);
        string result = Marshal.PtrToStringAnsi(resultPtr);

        if (result.StartsWith("Error:"))
        {
            EditorUtility.DisplayDialog("Error", result, "OK");
        }
        else
        {
            SaveClassToFile(result);
        }

        FreeString(resultPtr);
    }

    private void TestPerformance()
    {
        if (string.IsNullOrEmpty(jsonText))
        {
            EditorUtility.DisplayDialog("Error", "请先输入JSON", "OK");
            return;
        }

        Stopwatch stopwatch = new Stopwatch();
        double totalTime = 0;

        for (int i = 0; i < testIterations; i++)
        {
            stopwatch.Restart();
            System.IntPtr resultPtr = GenerateClassFromJson(jsonText, className);
            string result = Marshal.PtrToStringAnsi(resultPtr);
            stopwatch.Stop();

            FreeString(resultPtr);
            totalTime += stopwatch.Elapsed.TotalMilliseconds;
        }

        double averageTime = totalTime / testIterations;
        UnityEngine.Debug.Log($"C++ Version - Average Time ({testIterations} iterations): {averageTime:F4} ms");
        UnityEngine.Debug.Log($"C++ Version - Total Time ({testIterations} iterations): {totalTime:F4} ms");
    }

    private void SaveClassToFile(string classCode)
    {
        string path = EditorUtility.SaveFilePanel(
            "Save C# Class",
            "Assets",
            $"{className}.cs",
            "cs");

        if (!string.IsNullOrEmpty(path))
        {
            System.IO.File.WriteAllText(path, classCode);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", "成功", "OK");
        }
    }
}