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
using UnityEngine.Networking;

public class DBSourceManager : MonoBehaviour
{
    public string DBPath;
    public string DBName = "newDB";

    // 缓存加载的表数据
    private Dictionary<string, List<DBClass.tableBase>> _cachedTables = new Dictionary<string, List<DBClass.tableBase>>();

    // 正在加载的任务
    private HashSet<string> _loadingTasks = new HashSet<string>();

    private static Stack<(string executedSql, string undoSql)> _undoLog = new Stack<(string, string)>();
    public void Init()
    {
        DBPath = Path.Combine(Application.persistentDataPath, $"{DBName}.db");

        // 检查数据库文件是否已经存在于 persistentDataPath，如果不存在则从 StreamingAssets 复制
        if (!File.Exists(DBPath))
        {
            string sourcePath = Path.Combine(Application.streamingAssetsPath, $"{DBName}.db");
            Debug.Log($"文件已复制到 {DBPath}");
            if (File.Exists(sourcePath))
            {
                 File.Copy(sourcePath, DBPath, true);
             }
        }
    }

    private IEnumerator CopyDBFromStreamingAssets(UnityWebRequest request)
    {
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            File.WriteAllBytes(DBPath, request.downloadHandler.data);
            Debug.Log("复制成功");
        }
        else
        {
            Debug.LogError("失败");
        }
    }

    public List<DBClass.tableBase> GetCachedTableData(string tableName)
    {
        if (_cachedTables.ContainsKey(tableName))
        {
            return _cachedTables[tableName];
        }
        return new List<DBClass.tableBase>();
    }

    #region 读取
    /// <summary>
    /// 从tableName读取数据库，转化为C#类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="GetRowData">具体的数据库实现要重新写，后续做成脚本生成在DBClass.tableBase的具体类里</param>
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
            Debug.LogError($"无法读取 {tableName}: {ex.Message}");
            onComplete?.Invoke();
            yield break;
        }

        _cachedTables[tableName] = data.Cast<DBClass.tableBase>().ToList();
        _loadingTasks.Remove(tableName);
        yield return new WaitUntil(() => _cachedTables.ContainsKey(tableName));

        onComplete?.Invoke();
    }
    #endregion

    #region 保存
    /// <summary>
    /// 保存到tableName
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="SaveRowData">具体的数据库实现要重新写，后续做成脚本生成在DBClass.tableBase的具体类里</param>
    /// <param name="onComplete"></param>
    /// <param name="clearTable"></param>
    public void SaveDBAsync<T>(string tableName, Action<string, SqliteCommand, T> SaveRowData, Action onComplete = null, bool clearTable = false) where T : DBClass.tableBase
    {
        if (!_cachedTables.ContainsKey(tableName))
        {
            Debug.LogWarning($"{tableName} 保存了 .");
            return;
        }

        CoroutineManager.Instance.StartManagedCoroutine(SaveTableRoutine<T>(tableName, SaveRowData, onComplete, clearTable));
    }

    // 执行 SQL 语句并记录撤销日志
    public void ExecuteSql(string sql, string undoSql, Action<bool> onComplete = null)
    {
        CoroutineManager.Instance.StartManagedCoroutine(ExecuteSqlRoutine(sql, undoSql, onComplete));
    }

    private IEnumerator ExecuteSqlRoutine(string sql, string undoSql, Action<bool> onComplete)
    {
        bool success = false;
        try
        {
            using (SqliteConnection dbConnection = new SqliteConnection(new SqliteConnectionStringBuilder() { DataSource = DBPath }.ToString()))
            {
                dbConnection.Open();
                using (SqliteTransaction transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        using (SqliteCommand dbCommand = new SqliteCommand(sql, dbConnection, transaction))
                        {
                            dbCommand.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        _undoLog.Push((sql, undoSql)); // 记录撤销日志
                        success = true;
                        Debug.Log($"执行 SQL: {sql}, Undo SQL: {undoSql}");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Debug.LogError($"Error SQL: {sql}\nException: {ex.Message}");
                        success = false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Connection error: {ex.Message}");
            success = false;
        }

        yield return null;
        onComplete?.Invoke(success);
    }

    // 撤销最近的操作
    public static void UndoLastOperation(string dbPath, Action<bool> onComplete = null)
    {
        CoroutineManager.Instance.StartManagedCoroutine(UndoLastOperationRoutine(dbPath, onComplete));
    }

    private static IEnumerator UndoLastOperationRoutine(string dbPath, Action<bool> onComplete)
    {
        if (_undoLog.Count == 0)
        {
            Debug.Log("没有操作能undo.");
            onComplete?.Invoke(false);
            yield break;
        }

        var (executedSql, undoSql) = _undoLog.Pop();
        bool success = false;

        try
        {
            using (SqliteConnection dbConnection = new SqliteConnection(new SqliteConnectionStringBuilder() { DataSource = dbPath }.ToString()))
            {
                dbConnection.Open();
                using (SqliteTransaction transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        using (SqliteCommand dbCommand = new SqliteCommand(undoSql, dbConnection, transaction))
                        {
                            dbCommand.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        success = true;
                        Debug.Log($"Undo success: {undoSql}");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Debug.LogError($"Error undo SQL: {undoSql}\nException: {ex.Message}");
                        success = false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Connect error during undo: {ex.Message}");
            success = false;
        }

        yield return null;
        onComplete?.Invoke(success);
    }

    // 清空撤销日志
    public static void ClearUndoLog()
    {
        _undoLog.Clear();
    }
    /// <summary>
    /// 执行任意 SQL 查询并返回结果
    /// </summary>
    /// <param name="sql">SQL 查询语句</param>
    /// <param name="onComplete">查询完成后的回调，返回结果</param>
    public void ExecuteSqlQuery(string sql, Action<List<Dictionary<string, object>>> onComplete = null)
    {
        CoroutineManager.Instance.StartManagedCoroutine(ExecuteSqlQueryRoutine(sql, onComplete));
    }

    private IEnumerator ExecuteSqlQueryRoutine(string sql, Action<List<Dictionary<string, object>>> onComplete)
    {
        List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();

        try
        {
            using (SqliteConnection dbConnection = new SqliteConnection(new SqliteConnectionStringBuilder() { DataSource = DBPath }.ToString()))
            {
                dbConnection.Open();
                using (SqliteCommand dbCommand = new SqliteCommand(sql, dbConnection))
                {
                    using (IDataReader reader = dbCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader.GetValue(i);
                            }
                            results.Add(row);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error SQL: {sql}\nException: {ex.Message}");
            onComplete?.Invoke(null); // 出错时返回 null
            yield break;
        }

        yield return null;
        onComplete?.Invoke(results); // 返回查询结果
    }

    private IEnumerator SaveTableRoutine<T>(string tableName, Action<string, SqliteCommand, T> SaveRowData, Action onComplete = null, bool clearTable = false) where T : DBClass.tableBase
    {
        try
        {
            using (SqliteConnection dbConnection = new SqliteConnection(new SqliteConnectionStringBuilder() { DataSource = DBPath }.ToString()))
            {
                dbConnection.Open();

                // 可选：清空表数据
                if (clearTable)
                {
                    using (SqliteCommand clearCmd = new SqliteCommand($"DELETE FROM {tableName}", dbConnection))
                    {
                        clearCmd.ExecuteNonQuery();
                    }
                }

                // 保存数据
                List<T> data = _cachedTables[tableName].Cast<T>().ToList();
                foreach (var obj in data)
                {
                    using (SqliteCommand dbCommand = new SqliteCommand("", dbConnection))
                    {
                        // 通过委托设置SQL语句和参数
                        SaveRowData.Invoke(tableName, dbCommand, obj);
                        dbCommand.ExecuteNonQuery();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"无法保存到 {tableName}: {ex.Message}");
            onComplete?.Invoke();
            yield break;
        }

        yield return null;
        onComplete?.Invoke();
    }
    #endregion
}