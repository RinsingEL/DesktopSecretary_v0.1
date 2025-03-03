using System;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using Core.Framework.Event;
using Core.Framework.Resource;
using Unity.VisualScripting;
using Core.Framework.FGUI;

namespace Com.Module.Schedule
{
    public partial class UI_CalendarWindow : GComponent
    {
        private int _currentMonth; // 当前显示的月份
        private int _currentYear;  // 当前年份（用于跨年计算）
        private int _notCurrent;   // 本月第一天在周几（0=周日, 1=周一, ..., 6=周六）
        private int _daysInMonth;  // 本月天数
        private int _prevMonthDays; // 上个月天数
        private CalendarData _calendarData;
        public Queue<int> months = new Queue<int>(3);
        public DateTime selectDate;
        bool _isCanEdit;

        public void Init()
        {
            _currentMonth = DateTime.UtcNow.Month;
            _currentYear = DateTime.UtcNow.Year;

            m_IsShowDay.selectedIndex = 0;
            UpdateMonthInfo(); // 初始化月份相关数据

            // 初始化3个月队列
            months.Clear();
            months.Enqueue(GetMonth(_currentMonth - 1));
            months.Enqueue(_currentMonth);
            months.Enqueue(GetMonth(_currentMonth + 1));

            // 绑定按钮事件和渲染器
            m_prevBtn.onClick.Set(ShowPrevMonth);
            m_nextBtn.onClick.Set(ShowNextMonth);
            m_detailCollapseBtn.onClick.Set(FoldDetail);
            m_calendar.itemRenderer = OnRenderItem;
            m_taskList.itemRenderer = OnRenderTaskItem;
            m_EditBtn.onClick.Set(CanEdit);
            EventManager.Instance.AddEvent<CalendarData>(ClientEvent.UPDATE_CALENDAR_VIEW, UpdateView);
            EventManager.Instance.Trigger(ClientEvent.UPDATE_CALENDAR_REQUEST);
        }

        private void FoldDetail(EventContext context)
        {
            m_IsShowDay.selectedIndex = 0;
        }

        // 更新月份相关信息
        private void UpdateMonthInfo()
        {
            DateTime firstDay = new DateTime(_currentYear, _currentMonth, 1);
            _notCurrent = (int)firstDay.DayOfWeek; // 本月第一天是星期几
            _daysInMonth = DateTime.DaysInMonth(_currentYear, _currentMonth); // 本月天数
            _prevMonthDays = DateTime.DaysInMonth(_currentYear, GetMonth(_currentMonth - 1)); // 上个月天数
            UpdateMonthText();
        }

        // 处理月份循环
        private int GetMonth(int month)
        {
            if (month > 12) return month - 12;
            if (month < 1) return month + 12;
            return month;
        }

        // 更新视图
        private void UpdateView(CalendarData _calendarData)
        {
            UpdateMonthInfo();
            UpdateCalendarView(_calendarData);
            UpdateDetailView(_calendarData ,selectDate);
        }

        private void UpdateCalendarView(CalendarData _calendarData)
        {
            this._calendarData = _calendarData;
            m_calendar.numItems = 42;
        }


        // 渲染每个日期格子
        private void OnRenderItem(int index, GObject item)
        {
            var dayItem = item as UI_CalendarDayCom;
            int dayToShow;

            // 1. 上个月的尾巴
            if (index < _notCurrent)
            {
                dayToShow = _prevMonthDays - _notCurrent + index + 1;
                dayItem.Update(dayToShow, false, null); // false表示非本月
                dayItem.onClick.Clear();
            }
            // 2. 本月日期
            else if (index < _notCurrent + _daysInMonth)
            {
                dayToShow = index - _notCurrent + 1;
                var date = new DateTime(_currentYear, _currentMonth, dayToShow).ToShortDateString();
                List<DBClass.Task> tasks;
                _calendarData.tasksByDay.TryGetValue(date, out tasks);
                dayItem.Update(dayToShow, true, tasks); // true表示本月
                dayItem.onClick.Set(OnClickDayBtn);
            }
            // 3. 下个月的开头
            else
            {
                dayToShow = index - _notCurrent - _daysInMonth + 1;
                dayItem.Update(dayToShow, false, null); // false表示非本月
                dayItem.onClick.Clear();
            }
        }

        private void OnRenderTaskItem(int index, GObject item)
        {
            var taskItem = item as UI_detailTaskCom;
            taskItem.UpdateView(selectDate ,index, _calendarData.tasksByDay[selectDate.ToShortDateString()], _isCanEdit);
        }

        private void OnClickDayBtn(EventContext context)
        {
            var dayItem = context.sender as UI_CalendarDayCom;
            int dayToShow = int.Parse(dayItem.m_day.text);
            DateTime selectedDate = new DateTime(_currentYear, _currentMonth, dayToShow);

            // 切换到详情视图并刷新
            m_IsShowDay.selectedIndex = 1;
            UpdateDetailView(_calendarData , selectedDate);
        }
        private void CanEdit()
        {
            _isCanEdit = m_EditBtn.selected;
            UpdateDetailView(_calendarData,selectDate);
        }
        public void UpdateDetailView(CalendarData calendarData , DateTime selectDay)
        {
            selectDate = selectDay;
            m_dateTxt.text = selectDay.Day.ToString();
            if (calendarData.tasksByDay.TryGetValue(selectDay.ToShortDateString(), out var tasks))
            {
                m_taskList.numItems = tasks.Count;
            }
            else
            {
                m_taskList.numItems = 0; // 如果没有任务，显示空列表
            }
        }

        // 显示下个月
        public void ShowNextMonth(EventContext context)
        {
            _currentMonth++;
            if (_currentMonth > 12)
            {
                _currentMonth = 1;
                _currentYear++;
            }

            // 更新队列
            months.Dequeue();
            months.Enqueue(GetMonth(_currentMonth + 1));

            UpdateView(_calendarData);
            EventManager.Instance.Trigger<int>(ClientEvent.ON_MONTH_CHANGED, _currentMonth);
        }

        // 显示上个月
        public void ShowPrevMonth(EventContext context)
        {
            _currentMonth--;
            if (_currentMonth < 1)
            {
                _currentMonth = 12;
                _currentYear--;
            }

            // 更新队列
            months.Dequeue();
            months.Enqueue(GetMonth(_currentMonth - 1));

            UpdateView(_calendarData);
            EventManager.Instance.Trigger<int>(ClientEvent.ON_MONTH_CHANGED, _currentMonth);
        }

        // 更新月份显示文本
        private void UpdateMonthText()
        {
            m_month.text = $"{_currentMonth}月";
        }

        // 示例：监听月份变化
        private void OnMonthChanged(int newMonth)
        {
            UnityEngine.Debug.Log($"Month changed to: {newMonth}");
        }
        public void OnHide()
        {
            EventManager.Instance.RemoveEvent<CalendarData>(ClientEvent.UPDATE_CALENDAR_VIEW, UpdateView);
        }
    }
}