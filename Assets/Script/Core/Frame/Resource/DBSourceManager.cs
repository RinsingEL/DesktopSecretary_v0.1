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

    // 缓存加载的表数据
    private Dictionary<string, List<DBClass.tableBase>> _cachedTables = new Dictionary<string, List<DBClass.tableBase>>();

    // 正在加载的任务
    private HashSet<string> _loadingTasks = new HashSet<string>();
    public void Init()
    {
        DBPath = Path.Combine(Application.dataPath, "Resources", "DataBase", $"{DBName}.db");
    }
    public List<DBClass.tableBase> GetCachedTableData(string tableName)
    {
        // 如果缓存中有数据，返回数据，否则返回空列表
        if (_cachedTables.ContainsKey(tableName))
        {
            return _cachedTables[tableName];
        }
        return new List<DBClass.tableBase>();
    }
    #region 读取
    public void LoadDBAsync<T>(string tableName,Func<string, IDataReader, T> GetRowData,Action onComplete = null, string scope = "") where T : DBClass.tableBase
    {
        // 避免重复加载
        if (_loadingTasks.Contains(tableName)) return;
        _loadingTasks.Add(tableName);

        CoroutineManager.Instance.StartCoroutine(LoadTableRoutine<T>(tableName, GetRowData,onComplete, scope));
    }
    /// <summary>
    /// 加载表数据的协程
    /// </summary>
    /// <typeparam name="T">表的每列数据的映射类</typeparam>
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
                        // 根据表结构映射数据到对象
                        var rowData = GetRowData.Invoke(tableName, reader);
                        if (rowData != null)
                        {
                            data.Add(rowData);
                        }
                    }
                }
            }

            // 缓存数据
            _cachedTables[tableName] = data.Cast<DBClass.tableBase>().ToList();
        }
        catch (Exception ex)
        {
            // 发生错误时，打印日志，并执行错误回调（如果有）
            Debug.LogError($"Error while loading table {tableName}: {ex.Message}");
            onComplete?.Invoke();  // 在发生错误时调用 onComplete，告知外部处理失败
            yield break;  // 停止协程
        }
        // 缓存数据
        _cachedTables[tableName] = data.Cast<DBClass.tableBase>().ToList();

        // 移除加载任务
        _loadingTasks.Remove(tableName);
        yield return new WaitUntil(() => _cachedTables.ContainsKey(tableName));

        onComplete?.Invoke();

    }
    #endregion
    #region 保存
    // 保存指定表的数据
    public void SaveTableData(string tableName, List<DBClass.tableBase> newData)
    {
        if (!_cachedTables.ContainsKey(tableName))
        {
            Debug.LogWarning($"Table {tableName} is not loaded or cached.");
            return;
        }

        _cachedTables[tableName] = newData;

        // 调用保存表数据的协程
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
                    // 假设 obj 是一个包含数据的对象，你可以将其属性绑定到 SQL 参数
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
