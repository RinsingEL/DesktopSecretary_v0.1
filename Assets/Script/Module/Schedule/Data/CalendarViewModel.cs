using System;
using System.Collections.Generic;
using Core.Framework.Event;
using Core.Framework.Resource;

namespace Com.Module.Schedule
{
    public class CalendarViewModel
    {
        private CalendarData _calendarData;

        public CalendarViewModel()
        {
            _calendarData = new CalendarData();
            _calendarData.Load();
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

        public void ModifyTask(DateTime date, int index, string title, string description)
        {
            string key = date.ToShortDateString();
            if (_calendarData.tasksByDay.ContainsKey(key) && index < _calendarData.tasksByDay[key].Count)
            {
                var task = _calendarData.tasksByDay[key][index];
                task.Title = title;
                task.Description = description;
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
    }
}