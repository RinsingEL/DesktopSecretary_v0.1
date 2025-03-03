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
        if (_cachedTables.ContainsKey(tableName))
        {
            return _cachedTables[tableName];
        }
        return new List<DBClass.tableBase>();
    }

    #region ��ȡ
    /// <summary>
    /// ��tableName��ȡ���ݿ⣬ת��ΪC#��
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="GetRowData">��������ݿ�ʵ��Ҫ����д���������ɽű�������DBClass.tableBase�ľ�������</param>
    /// <param name="onComplete"></param>
    /// <param name="scope"></param>
    public void LoadDBAsync<T>(string tableName, Func<string, IDataReader, T> GetRowData, Action onComplete = null, string scope = "") where T : DBClass.tableBase
    {
        if (_loadingTasks.Contains(tableName)) return;
        _loadingTasks.Add(tableName);

        CoroutineManager.Instance.StartManagedCoroutine(LoadTableRoutine<T>(tableName, GetRowData, onComplete, scope));
    }

    private IEnumerator LoadTableRoutine<T>(string tableName, Func<string, IDataReader, T> GetRowData, Action onComplete = null, string scope = "") where T : DBClass.tableBase
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
                        var rowData = GetRowData.Invoke(tableName, reader);
                        if (rowData != null)
                        {
                            data.Add(rowData);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error while loading table {tableName}: {ex.Message}");
            onComplete?.Invoke();
            yield break;
        }

        _cachedTables[tableName] = data.Cast<DBClass.tableBase>().ToList();
        _loadingTasks.Remove(tableName);
        yield return new WaitUntil(() => _cachedTables.ContainsKey(tableName));

        onComplete?.Invoke();
    }
    #endregion

    #region ����
    /// <summary>
    /// ���浽tableName
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="SaveRowData">��������ݿ�ʵ��Ҫ����д���������ɽű�������DBClass.tableBase�ľ�������</param>
    /// <param name="onComplete"></param>
    /// <param name="clearTable"></param>
    public void SaveDBAsync<T>(string tableName, Action<string, SqliteCommand, T> SaveRowData, Action onComplete = null, bool clearTable = false) where T : DBClass.tableBase
    {
        if (!_cachedTables.ContainsKey(tableName))
        {
            Debug.LogWarning($"Table {tableName} is not loaded or cached.");
            return;
        }

        CoroutineManager.Instance.StartManagedCoroutine(SaveTableRoutine<T>(tableName, SaveRowData, onComplete, clearTable));
    }

    //public void SaveTableData(string tableName, List<DBClass.tableBase> newData)
    //{
    //    if (!_cachedTables.ContainsKey(tableName))
    //    {
    //        Debug.LogWarning($"Table {tableName} is not loaded or cached.");
    //        return;
    //    }

    //    _cachedTables[tableName] = newData;
    //    CoroutineManager.Instance.StartManagedCoroutine(SaveTableRoutine<DBClass.tableBase>(tableName,
    //        (table, cmd, data) => { },
    //        null));
    //}�о�û��Ҫ��дһ�α���ȫ����?linww

    private IEnumerator SaveTableRoutine<T>(string tableName, Action<string, SqliteCommand, T> SaveRowData, Action onComplete = null, bool clearTable = false) where T : DBClass.tableBase
    {
        try
        {
            using (SqliteConnection dbConnection = new SqliteConnection(new SqliteConnectionStringBuilder() { DataSource = DBPath }.ToString()))
            {
                dbConnection.Open();

                // ��ѡ����ձ�����
                if (clearTable)
                {
                    using (SqliteCommand clearCmd = new SqliteCommand($"DELETE FROM {tableName}", dbConnection))
                    {
                        clearCmd.ExecuteNonQuery();
                    }
                }

                // ��������
                List<T> data = _cachedTables[tableName].Cast<T>().ToList();
                foreach (var obj in data)
                {
                    using (SqliteCommand dbCommand = new SqliteCommand("", dbConnection))
                    {
                        // ͨ��ί������SQL���Ͳ���
                        SaveRowData.Invoke(tableName, dbCommand, obj);
                        dbCommand.ExecuteNonQuery();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error while saving table {tableName}: {ex.Message}");
            onComplete?.Invoke();
            yield break;
        }

        yield return null;
        onComplete?.Invoke();
    }
    #endregion
}