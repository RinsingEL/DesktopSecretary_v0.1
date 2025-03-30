using System;
using System.Collections.Generic;
using Core.Framework.Event;
using Core.Framework.Resource;
using UnityEngine;

namespace Com.Module.Schedule
{
    public class CalendarViewModel
    {
        private CalendarData _calendarData;

        public CalendarViewModel()
        {
            _calendarData = new CalendarData();
            _calendarData.Load();
            EventManager.Instance.AddEvent(ClientEvent.UPDATE_CALENDAR_INFO, OnUpdateInfo);
            Application.quitting += OnApplicationQuit;
        }

        private void OnApplicationQuit()
        {
            Cleanup();
        }

        private void OnUpdateInfo()
        {
            _calendarData.Load();
        }

        private static CalendarViewModel instance;
        public static CalendarViewModel Instance
        {
            get {
                if (instance == null)
                    instance = new CalendarViewModel();
                return instance;
            }
        }
        public List<DBClass.Task> GetTasksForDay(DateTime date)
        {
            string key = date.ToShortDateString();
            if (_calendarData.tasksByDay.TryGetValue(key, out var tasks))
            {
                return tasks;
            }
            return new List<DBClass.Task>();
        }

        public void ModifyTask(DateTime date, int index, string title, string description, DateTime startDate, DateTime dueTime, int priority = 2, int status = 0)
        {
            string key = date.ToShortDateString();
            if (_calendarData.tasksByDay.ContainsKey(key) && index < _calendarData.tasksByDay[key].Count)
            {
                var task = _calendarData.tasksByDay[key][index];
                task.Title = title;
                task.Description = description;
                task.DueDate = dueTime;
                task.StartedAt = startDate;
                task.Priority = priority;
                task.Status = status;
                task.UpdatedAt = DateTime.Now;
                _calendarData.Save();
                EventManager.Instance.Trigger(ClientEvent.UPDATE_CALENDAR_VIEW);
            }
        }

        public void AddNewTask(DateTime date, string title, string description, DateTime startDate, DateTime dueTime, int priority = 2, int status = 0)
        {
            string key = date.ToShortDateString();
            if (!_calendarData.tasksByDay.ContainsKey(key))
            {
                _calendarData.tasksByDay[key] = new List<DBClass.Task>();
            }

            var task = new DBClass.Task
            {
                TaskID = Guid.NewGuid().ToString("N"),
                Title = title,
                Description = description,
                StartedAt = startDate,
                DueDate = dueTime,
                Priority = priority,
                Status = status,
                UpdatedAt = DateTime.Now
            };

            // Ìí¼Óµ½ tasks ºÍ tasksByDay
            _calendarData.tasks.Add(task);
            _calendarData.tasksByDay[key].Add(task);

            _calendarData.Save();
            EventManager.Instance.Trigger(ClientEvent.UPDATE_CALENDAR_VIEW);
        }

        public List<DBClass.Task> GetAllTasks()
        {
            return _calendarData.tasks;
        }

        public void UpdateTask(DBClass.Task task)
        {
            task.UpdatedAt = DateTime.Now;
            _calendarData.Save();
            EventManager.Instance.Trigger(ClientEvent.UPDATE_CALENDAR_VIEW);
        }
        public void DeleteTask(DateTime date, int index)
        {
            string key = date.ToShortDateString();
            if (_calendarData.tasksByDay.ContainsKey(key) && index < _calendarData.tasksByDay[key].Count)
            {
                var task = _calendarData.tasksByDay[key][index];
                _calendarData.tasks.Remove(task);
                _calendarData.tasksByDay[key].RemoveAt(index);
                _calendarData.Save();
                EventManager.Instance.Trigger(ClientEvent.UPDATE_CALENDAR_VIEW);
            }
        }

        public void Cleanup()
        {
            EventManager.Instance.RemoveEvent(ClientEvent.UPDATE_CALENDAR_INFO, OnUpdateInfo);
            Application.quitting -= OnApplicationQuit;
        }
    }
}