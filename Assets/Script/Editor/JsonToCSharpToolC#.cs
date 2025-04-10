using UnityEditor;
using UnityEngine;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

public class JsonToCSharpClassGenerator_CSharp : EditorWindow
{
    private string jsonText = "";
    private string className = "GeneratedClass";
    private Vector2 scrollPos;
    private StringBuilder allClassesCode;
    private int testIterations = 1; // 测试次数

    [MenuItem("Tools/JSON to C# Class Generator (C#)")]
    public static void ShowWindow()
    {
        GetWindow<JsonToCSharpClassGenerator_CSharp>("JSON to C# Class Generator (C#)");
    }

    private void OnGUI()
    {
        GUILayout.Label("JSON to C# Class Generator (C#)", EditorStyles.boldLabel);

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

    private void GenerateClassFromJson()
    {
        if (string.IsNullOrEmpty(jsonText))
        {
            EditorUtility.DisplayDialog("Error", "请先输入JSON", "OK");
            return;
        }

        try
        {
            allClassesCode = new StringBuilder();
            allClassesCode.AppendLine("using System;");
            allClassesCode.AppendLine("using UnityEngine;");
            allClassesCode.AppendLine();

            JObject jsonObj = JObject.Parse(jsonText);
            if (jsonObj == null) throw new System.Exception("转化不成 JSON");

            GenerateClassCode(className, jsonObj);
            SaveClassToFile(allClassesCode.ToString());
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"无法转化: {e.Message}为CS类", "OK");
            UnityEngine.Debug.LogError($"JSON Parse Error: {e.StackTrace}");
        }
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
            allClassesCode = new StringBuilder();
            allClassesCode.AppendLine("using System;");
            allClassesCode.AppendLine("using UnityEngine;");
            allClassesCode.AppendLine();

            JObject jsonObj = JObject.Parse(jsonText);
            GenerateClassCode(className, jsonObj);
            stopwatch.Stop();

            totalTime += stopwatch.Elapsed.TotalMilliseconds;
        }

        double averageTime = totalTime / testIterations;
        UnityEngine.Debug.Log($"C# Version - Average Time ({testIterations} iterations): {averageTime:F4} ms");
        UnityEngine.Debug.Log($"C# Version - totalTime ({testIterations} iterations): {totalTime:F4} ms");
    }

    private void GenerateClassCode(string className, JObject jsonObj)
    {
        // 添加类定义到总代码中
        allClassesCode.AppendLine($"[Serializable]");
        allClassesCode.AppendLine($"public class {className}");
        allClassesCode.AppendLine("{");

        foreach (var property in jsonObj.Properties())
        {
            string fieldName = property.Name;
            JToken value = property.Value;
            string fieldType = GetFieldType(fieldName, value, className);

            allClassesCode.AppendLine($"\tpublic {fieldType} {fieldName};");
        }

        allClassesCode.AppendLine("}");
        allClassesCode.AppendLine(); // 添加空行分隔类
    }

    private string GetFieldType(string fieldName, JToken value, string parentClassName)
    {
        if (value == null) return "string";

        switch (value.Type)
        {
            case JTokenType.String:
                return "string";
            case JTokenType.Boolean:
                return "bool";
            case JTokenType.Integer:
                return "int";
            case JTokenType.Float:
                return "float";
            case JTokenType.Object:
                string nestedClassName = $"{parentClassName}_{fieldName}";
                GenerateNestedClass(nestedClassName, value as JObject);
                return nestedClassName;
            case JTokenType.Array:
                JArray array = value as JArray;
                if (array.Count > 0)
                {
                    string elementType = GetArrayElementType(array[0]);
                    return $"{elementType}[]";
                }
                return "string[]";
            default:
                return "string";
        }
    }

    private string GetArrayElementType(JToken element)
    {
        switch (element.Type)
        {
            case JTokenType.String:
                return "string";
            case JTokenType.Boolean:
                return "bool";
            case JTokenType.Integer:
                return "int";
            case JTokenType.Float:
                return "float";
            default:
                return "string";
        }
    }

    private void GenerateNestedClass(string nestedClassName, JObject jsonObj)
    {
        // 将嵌套类添加到总代码中
        allClassesCode.AppendLine($"[Serializable]");
        allClassesCode.AppendLine($"public class {nestedClassName}");
        allClassesCode.AppendLine("{");

        foreach (var property in jsonObj.Properties())
        {
            string fieldName = property.Name;
            JToken value = property.Value;
            string fieldType = GetFieldType(fieldName, value, nestedClassName);

            allClassesCode.AppendLine($"\tpublic {fieldType} {fieldName};");
        }

        allClassesCode.AppendLine("}");
        allClassesCode.AppendLine(); // 添加空行分隔类
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
            File.WriteAllText(path, classCode);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", "成功", "OK");
        }
    }
}