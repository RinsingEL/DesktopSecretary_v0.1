using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Com.Module.Watcher;
using Core.Framework.Resource;
using Core.Framework.Utility;
using Mono.Data.Sqlite;
using UnityEngine;
using System.Linq;

public class DBExtend : MonoBehaviour
{
    private List<DBClass.TaskEvaluation> evaluations = new List<DBClass.TaskEvaluation>();
    private Dictionary<string, List<DBClass.TaskEvaluation>> evaluationsByTask = new Dictionary<string, List<DBClass.TaskEvaluation>>();

    public void LoadEvaluations(Action onComplete = null)
    {
        ResourcesManager.Instance.DBSourceManager.LoadDBAsync<DBClass.TaskEvaluation>(
            "TaskEvaluation",
            GetEvaluationRow,
            () => {
                var cachedData = ResourcesManager.Instance.DBSourceManager.GetCachedTableData("TaskEvaluation");
                evaluations = cachedData.Select(e => (DBClass.TaskEvaluation)e).ToList();
                CoroutineManager.Instance.StartManagedCoroutine(BuildEvaluationCache());
                onComplete?.Invoke();
            }
        );
    }

    private IEnumerator BuildEvaluationCache()
    {
        evaluationsByTask.Clear();
        
        foreach (var eval in evaluations)
        {
            if (!evaluationsByTask.ContainsKey(eval.Tasked))
            {
                evaluationsByTask[eval.Tasked] = new List<DBClass.TaskEvaluation>();
            }
            evaluationsByTask[eval.Tasked].Add(eval);
            
            yield return null; // 分帧处理
        }
    }

    private DBClass.TaskEvaluation GetEvaluationRow(string tableName, IDataReader reader)
    {
        return new DBClass.TaskEvaluation
        {
            EvaluationID = reader.GetString(0),
            Tasked = reader.GetString(1),
            EvaluationContent = reader.IsDBNull(2) ? null : reader.GetString(2),
            Duration = reader.GetInt32(3),
            EvaluationTime = reader.GetDateTime(4)
        };
    }

    public void SaveEvaluations()
    {
        var cachedData = ResourcesManager.Instance.DBSourceManager.GetCachedTableData("TaskEvaluation");
        cachedData.Clear();
        cachedData.AddRange(evaluations.Cast<DBClass.tableBase>());

        ResourcesManager.Instance.DBSourceManager.SaveDBAsync<DBClass.TaskEvaluation>(
            "TaskEvaluation",
            SaveEvaluationRow,
            () => Debug.Log("任务评估数据保存成功")
        );
    }

    private void SaveEvaluationRow(string tableName, SqliteCommand cmd, DBClass.TaskEvaluation eval)
    {
        cmd.CommandText = $@"INSERT OR REPLACE INTO {tableName} 
            (evaluationid, tasked, evaluationcontent, duration, evaluationtime)
            VALUES (@evalId, @tasked, @content, @duration, @evalTime)";

        cmd.Parameters.AddWithValue("@evalId", eval.EvaluationID);
        cmd.Parameters.AddWithValue("@tasked", eval.Tasked);
        cmd.Parameters.AddWithValue("@content", eval.EvaluationContent ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@duration", eval.Duration);
        cmd.Parameters.AddWithValue("@evalTime", eval.EvaluationTime);
    }

    // 获取任务评估数据
    public List<DBClass.TaskEvaluation> GetEvaluationsForTask(string taskId)
    {
        return evaluationsByTask.TryGetValue(taskId, out var evals) ? evals : new List<DBClass.TaskEvaluation>();
    }

}
