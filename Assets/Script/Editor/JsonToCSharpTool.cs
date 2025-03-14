using UnityEditor;
using UnityEngine;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;

public class JsonToCSharpClassGenerator : EditorWindow
{
    private string jsonText = "";
    private string className = "GeneratedClass";
    private Vector2 scrollPos;
    private StringBuilder allClassesCode; // 用于收集所有类的代码

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

    private void GenerateClassFromJson()
    {
        if (string.IsNullOrEmpty(jsonText))
        {
            EditorUtility.DisplayDialog("Error", "请先输入JSON", "OK");
            return;
        }

        try
        {
            // 初始化 StringBuilder 用于收集所有代码
            allClassesCode = new StringBuilder();
            allClassesCode.AppendLine("using System;");
            allClassesCode.AppendLine("using UnityEngine;");
            allClassesCode.AppendLine();

            JObject jsonObj = JObject.Parse(jsonText);
            if (jsonObj == null)
            {
                throw new System.Exception("转化不成 JSON");
            }

            GenerateClassCode(className, jsonObj);
            SaveClassToFile(allClassesCode.ToString());
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"无法转化: {e.Message}为CS类", "OK");
            Debug.LogError($"JSON Parse Error: {e.StackTrace}");
        }
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