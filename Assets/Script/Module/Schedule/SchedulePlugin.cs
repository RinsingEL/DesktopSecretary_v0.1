using Core.Framework.Event;
using Core.Framework.Plugin;
using System;

namespace Com.Module.Schedule
{
    public class SchedulePlugin : PluginBase
    {
        private CalendarData _calendarData = new CalendarData();
        private static SchedulePlugin instance;
        public static SchedulePlugin Instance
        {
            get
            {
                if (instance == null)
                    instance = new SchedulePlugin();
                return instance;
            }
        }

        protected override void OnRegister()
        {
            _calendarData.Load();
            EventManager.Instance.AddEvent(ClientEvent.UPDATE_CALENDAR_REQUEST,OnCalendarInfoResponse);
            EventManager.Instance.AddEvent(ClientEvent.UPDATE_CALENDAR_INFO, OnUpdateInfo);
        }

        public void ModifyCalendarData(DateTime date, string title, string des, int index)
        {
            _calendarData.tasksByDay[date.ToShortDateString()][index].Title = title;
            _calendarData.tasksByDay[date.ToShortDateString()][index].Description = des;
        }
        public void AddNewTaskData(DateTime date,string title,string des,DateTime startDate,DateTime dueTime)
        {

            Core.Framework.Resource.DBClass.Task item = new Core.Framework.Resource.DBClass.Task();
            item.Title = title;
            item.Description = des;
            item.StartedAt = startDate;
            item.DueDate = dueTime;
            _calendarData.tasksByDay[date.ToShortDateString()].Add(item);
        }
        private void OnUpdateInfo()
        {
            _calendarData.Save();
        }

        private void OnCalendarInfoResponse()
        {
            EventManager.Instance.Trigger<CalendarData>(ClientEvent.UPDATE_CALENDAR_VIEW, _calendarData);
        }

        protected override void OnUninstall()
        {
            EventManager.Instance.RemoveEvent(ClientEvent.UPDATE_CALENDAR_INFO, OnUpdateInfo);
            EventManager.Instance.RemoveEvent(ClientEvent.UPDATE_CALENDAR_REQUEST, OnCalendarInfoResponse);
        }

        protected override void OnUpdate()
        {
            throw new NotImplementedException();
        }
    }
}