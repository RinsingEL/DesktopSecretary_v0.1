using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.Framework.Resource;
namespace Schedule
{
    public class CalendarData
    {
        List<DBClass.Task> tasks = new List<DBClass.Task>();
        public void OnInit()
        {
            ResourcesManager.Instance.DBSourceManager.LoadDBAsync<DBClass.Task>("Tasks", GetEventRow, OnDataLoaded);
        }
        private void OnDataLoaded()
        {
            // 获取缓存中的任务数据
            var cachedData = ResourcesManager.Instance.DBSourceManager.GetCachedTableData("Tasks");

            if (cachedData.Count > 0)
            {
                // 将缓存数据转成 List<DBClass.Task>
                tasks = cachedData.Cast<DBClass.Task>().ToList();
            }
        }
        private DBClass.Task GetEventRow(string tableName, IDataReader reader)
        {
            var task = new DBClass.Task
            {

                TaskID = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                Priority = reader.IsDBNull(3) ? 2 : reader.GetInt32(3), // 默认为2
                Status = reader.IsDBNull(4) ? 0 : reader.GetInt32(4), // 默认为0
                DueDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                CreatedAt = reader.GetDateTime(6),
                UpdatedAt = reader.GetDateTime(7)
            };
            return task;
        }
    }
}