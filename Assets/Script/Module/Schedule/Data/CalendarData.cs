using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Com.Module.Schedule;
using Core.Framework.FGUI;
using Core.Framework.Resource;
using Core.Framework.Utility;
using Mono.Data.Sqlite;
using UnityEngine;

namespace Com.Module.Schedule
{
    public class CalendarData
    {
        public List<DBClass.Task> tasks = new List<DBClass.Task>();
        public Dictionary<string, List<DBClass.Task>> tasksByDay = new Dictionary<string, List<DBClass.Task>>(); // 按日期缓存
        public bool IsInitialized = false;

        // 按日构建任务字典
        public IEnumerator BuildTaskCache()
        {
            tasksByDay.Clear();
            int tasksPerFrame = 100;
            int processedCount = 0;

            foreach (var task in tasks)
            {
                string day = task.StartedAt.ToShortDateString();
                if (!tasksByDay.TryGetValue(day, out var taskList))
                {
                    taskList = new List<DBClass.Task>();
                    tasksByDay[day] = taskList;
                }
                taskList.Add(task);

                processedCount++;
                if (processedCount % tasksPerFrame == 0)
                {
                    yield return null;
                }
            }

            yield return null;
            IsInitialized = true;
        }

        private IEnumerator BuildTaskCacheRoutine()
        {
            yield return CoroutineManager.Instance.StartManagedCoroutine(BuildTaskCache());
        }

        public void Load()
        {
            ResourcesManager.Instance.DBSourceManager.LoadDBAsync<DBClass.Task>("Tasks", GetEventRow, OnDataLoaded);
        }

        private void OnDataLoaded()
        {
            var cachedData = ResourcesManager.Instance.DBSourceManager.GetCachedTableData("Tasks");
            tasks = cachedData.Cast<DBClass.Task>().ToList();
            CoroutineManager.Instance.StartManagedCoroutine(BuildTaskCacheRoutine());
        }

        private DBClass.Task GetEventRow(string tableName, IDataReader reader)
        {
            var task = new DBClass.Task
            {
                TaskID = reader.GetString(0),
                Title = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                Priority = reader.IsDBNull(3) ? 2 : reader.GetInt32(3),
                Status = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                DueDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                StartedAt = reader.GetDateTime(6),
                UpdatedAt = reader.GetDateTime(7)
            };
            return task;
        }

        public void Save()
        {
            // 同步到 _cachedTables
            ResourcesManager.Instance.DBSourceManager.GetCachedTableData("Tasks").Clear();
            ResourcesManager.Instance.DBSourceManager.GetCachedTableData("Tasks").AddRange(tasks.Cast<DBClass.tableBase>());

            // 保存到数据库
            ResourcesManager.Instance.DBSourceManager.SaveDBAsync<DBClass.Task>("Tasks", SaveEventRow, OnSaveComplete);
        }

        private void OnSaveComplete()
        {
            Debug.Log("成功保存tasks");
        }

        private void SaveEventRow(string tableName, SqliteCommand cmd, DBClass.Task task)
        {
            cmd.CommandText = $"INSERT OR REPLACE INTO {tableName} (TaskID, Title, Description, Priority, Status, DueDate, StartedAt, UpdatedAt) " +
                              $"VALUES (@taskID, @title, @description, @priority, @status, @dueDate, @startedAt, @updatedAt)";

            cmd.Parameters.AddWithValue("@taskID", task.TaskID);
            cmd.Parameters.AddWithValue("@title", task.Title ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@description", task.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@priority", task.Priority);
            cmd.Parameters.AddWithValue("@status", task.Status);
            cmd.Parameters.AddWithValue("@dueDate", task.DueDate.HasValue ? task.DueDate.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@startedAt", task.StartedAt);
            cmd.Parameters.AddWithValue("@updatedAt", task.UpdatedAt);
        }
    }
}