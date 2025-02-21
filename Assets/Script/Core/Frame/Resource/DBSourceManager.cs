using Core.Framework.Config;
using Core.Framework.Resource;
using Core.Framework.Utility;
using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

public class DBSourceManager : MonoBehaviour
{
    public string DBPath;
    public string DBName = "newDB";

    // ������صı�����
    private Dictionary<string, List<DBClass.tableBase>> _cachedTables = new Dictionary<string, List<DBClass.tableBase>>();

    // ���ڼ��ص�����
    private HashSet<string> _loadingTasks = new HashSet<string>();
    public void Init()
    {
        DBPath = Path.Combine(Application.dataPath, "Resources", "DataBase", $"{DBName}.db");
    }
    public List<DBClass.tableBase> GetCachedTableData(string tableName)
    {
        // ��������������ݣ��������ݣ����򷵻ؿ��б�
        if (_cachedTables.ContainsKey(tableName))
        {
            return _cachedTables[tableName];
        }
        return new List<DBClass.tableBase>();
    }
    #region ��ȡ
    public void LoadDBAsync<T>(string tableName,Func<string, IDataReader, T> GetRowData,Action onComplete = null, string scope = "") where T : DBClass.tableBase
    {
        // �����ظ�����
        if (_loadingTasks.Contains(tableName)) return;
        _loadingTasks.Add(tableName);

        CoroutineManager.Instance.StartCoroutine(LoadTableRoutine<T>(tableName, GetRowData,onComplete, scope));
    }
    /// <summary>
    /// ���ر����ݵ�Э��
    /// </summary>
    /// <typeparam name="T">���ÿ�����ݵ�ӳ����</typeparam>
    /// <param name="tableName"></param>
    /// <param name="GetRowData"></param>
    /// <returns></returns>
    private IEnumerator LoadTableRoutine<T>(string tableName, System.Func<string, IDataReader,T> GetRowData, Action onComplete = null, string scope = "") where T : DBClass.tableBase
    {
        List<T> data = new List<T>();
        try
        {
            using (SqliteConnection dbConnection = new SqliteConnection(new SqliteConnectionStringBuilder() { DataSource = DBPath }.ToString()))
            {
                dbConnection.Open();
                string sql = scope != "" ? $"SELECT * FROM {tableName} WHERE {scope}" : $"SELECT * FROM {tableName}";
                SqliteCommand dbCommand = new SqliteCommand(sql, dbConnection);

                using (IDataReader reader = dbCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // ���ݱ�ṹӳ�����ݵ�����
                        var rowData = GetRowData.Invoke(tableName, reader);
                        if (rowData != null)
                        {
                            data.Add(rowData);
                        }
                    }
                }
            }

            // ��������
            _cachedTables[tableName] = data.Cast<DBClass.tableBase>().ToList();
        }
        catch (Exception ex)
        {
            // ��������ʱ����ӡ��־����ִ�д���ص�������У�
            Debug.LogError($"Error while loading table {tableName}: {ex.Message}");
            onComplete?.Invoke();  // �ڷ�������ʱ���� onComplete����֪�ⲿ����ʧ��
            yield break;  // ֹͣЭ��
        }
        // ��������
        _cachedTables[tableName] = data.Cast<DBClass.tableBase>().ToList();

        // �Ƴ���������
        _loadingTasks.Remove(tableName);
        yield return new WaitUntil(() => _cachedTables.ContainsKey(tableName));

        onComplete?.Invoke();

    }
    #endregion
    #region ����
    // ����ָ���������
    public void SaveTableData(string tableName, List<DBClass.tableBase> newData)
    {
        if (!_cachedTables.ContainsKey(tableName))
        {
            Debug.LogWarning($"Table {tableName} is not loaded or cached.");
            return;
        }

        _cachedTables[tableName] = newData;

        // ���ñ�������ݵ�Э��
        StartCoroutine(SaveTableRoutine(tableName));
    }
    public void SaveDBAsync(string tableName)
    {
        if (!_cachedTables.ContainsKey(tableName))
        {
            Debug.LogWarning($"Table {tableName} is not loaded or cached.");
            return;
        }

        CoroutineManager.Instance.StartCoroutine(SaveTableRoutine(tableName));
    }

    private IEnumerator SaveTableRoutine(string tableName)
    {
        List<DBClass.tableBase> data = _cachedTables[tableName];

        string connectionString = $"URI=file:{DBPath}";
        using (SqliteConnection dbConnection = new SqliteConnection(new SqliteConnectionStringBuilder() { DataSource = DBPath }.ToString()))
        {
            dbConnection.Open();

            foreach (var obj in data)
            {
                string sql = $"INSERT INTO {tableName} (column1, column2, ...) VALUES (@value1, @value2, ...)";

                using (SqliteCommand dbCommand = new SqliteCommand(sql, dbConnection))
                {
                    // ���� obj ��һ���������ݵĶ�������Խ������԰󶨵� SQL ����
                    // dbCommand.Parameters.AddWithValue("@value1", obj.Property1);
                    // dbCommand.Parameters.AddWithValue("@value2", obj.Property2);
                    // ...

                    dbCommand.ExecuteNonQuery();
                }
            }
        }

        yield return null;
    }
    #   endregion
}
