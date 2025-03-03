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

namespace Com.Module.Schedule
{
    public class CalendarData
    {
        List<DBClass.Task> tasks = new List<DBClass.Task>();
        public Dictionary<string, List<DBClass.Task>> tasksByDay = new Dictionary<string, List<DBClass.Task>>(); // �����ڻ���
        public bool IsInitialized = false;

        // Э�̰汾�����չ��������ֵ�
        public IEnumerator BuildTaskCache()
        {
            tasksByDay.Clear();
            int tasksPerFrame = 100; // ÿ֡����������������ɵ�����ƽ������
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
                    yield return null; // ÿ����tasksPerFrame�����񣬵ȴ���һ֡
                }
            }

            yield return null; // ȷ�����һ֡���
            IsInitialized = true;
        }

        // ��װЭ�̲�������ɺ���߼�
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
            // ��ȡ�����е���������
            var cachedData = ResourcesManager.Instance.DBSourceManager.GetCachedTableData("Tasks");

            if (cachedData.Count > 0)
            {
                // ����������תΪ List<DBClass.Task>
                tasks = cachedData.Cast<DBClass.Task>().ToList();
                CoroutineManager.Instance.StartManagedCoroutine(BuildTaskCacheRoutine());
            }
        }

        private DBClass.Task GetEventRow(string tableName, IDataReader reader)
        {
            var task = new DBClass.Task
            {
                TaskID = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                Priority = reader.IsDBNull(3) ? 2 : reader.GetInt32(3), // Ĭ��Ϊ2
                Status = reader.IsDBNull(4) ? 0 : reader.GetInt32(4), // Ĭ��Ϊ0
                DueDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                StartedAt = reader.GetDateTime(6),
                UpdatedAt = reader.GetDateTime(7)
            };
            return task;
        }
        public void Save()
        {
            ResourcesManager.Instance.DBSourceManager.SaveDBAsync<DBClass.Task>("Tasks",SaveEventRow);
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