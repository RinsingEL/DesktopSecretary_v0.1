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
    private StringBuilder allClassesCode; // �����ռ�������Ĵ���

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
            EditorUtility.DisplayDialog("Error", "��������JSON", "OK");
            return;
        }

        try
        {
            // ��ʼ�� StringBuilder �����ռ����д���
            allClassesCode = new StringBuilder();
            allClassesCode.AppendLine("using System;");
            allClassesCode.AppendLine("using UnityEngine;");
            allClassesCode.AppendLine();

            JObject jsonObj = JObject.Parse(jsonText);
            if (jsonObj == null)
            {
                throw new System.Exception("ת������ JSON");
            }

            GenerateClassCode(className, jsonObj);
            SaveClassToFile(allClassesCode.ToString());
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"�޷�ת��: {e.Message}ΪCS��", "OK");
            Debug.LogError($"JSON Parse Error: {e.StackTrace}");
        }
    }

    private void GenerateClassCode(string className, JObject jsonObj)
    {
        // ����ඨ�嵽�ܴ�����
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
        allClassesCode.AppendLine(); // ��ӿ��зָ���
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
        // ��Ƕ������ӵ��ܴ�����
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
        allClassesCode.AppendLine(); // ��ӿ��зָ���
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
            EditorUtility.DisplayDialog("Success", "�ɹ�", "OK");
        }
    }
}