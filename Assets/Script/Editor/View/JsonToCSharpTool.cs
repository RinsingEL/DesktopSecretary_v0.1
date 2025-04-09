using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;

public class JsonToCSharpClassGenerator : EditorWindow
{
    private string jsonText = "";
    private string className = "GeneratedClass";
    private Vector2 scrollPos;

    [MenuItem("Tools/JSON to C# Class Generator")]
    public static void ShowWindow()
    {
        GetWindow<JsonToCSharpClassGenerator>("JSON to C# Class Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("JSON to C# Class Generator", EditorStyles.boldLabel);

        className = EditorGUILayout.TextField("Class Name", className);

        GUILayout.Label("Input JSON:");
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        jsonText = EditorGUILayout.TextArea(jsonText, GUILayout.Height(200));
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Generate C# Class"))
        {
            GenerateClassFromJson();
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

        FreeString(resultPtr); // 释放C++分配的内存
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