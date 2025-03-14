using System;
using System.Collections.Generic;
using Core.Framework.Event;
using Core.Framework.FGUI;
using FairyGUI;
using UnityEngine;

namespace Com.Module.Schedule
{
    public partial class UI_CalendarWindow : GComponent
    {
        private CalendarViewModel _viewModel;
        private int _currentMonth;
        private int _currentYear;
        private int _notCurrent;
        private int _daysInMonth;
        private int _prevMonthDays;
        public Queue<int> months = new Queue<int>(3);
        private DateTime _selectDate;
        private bool _isCanEdit;

        public void Init(CalendarViewModel viewModel)
        {
            _viewModel = viewModel;
            // 绑定事件和渲染器
            m_prevBtn.onClick.Set(ShowPrevMonth);
            m_nextBtn.onClick.Set(ShowNextMonth);
            m_detailCollapseBtn.onClick.Set(FoldDetail);
            m_calendar.itemRenderer = OnRenderItem;
            m_taskList.itemRenderer = OnRenderTaskItem;
            m_addBtn.onClick.Set(ShowAddTaskWindow);
            // 订阅 ViewModel 的事件
            EventManager.Instance.AddEvent(ClientEvent.UPDATE_CALENDAR_VIEW, UpdateView);
        }

        private void ShowAddTaskWindow(EventContext context)
        {
            EditWindow.EditWindowParam param = new EditWindow.EditWindowParam()
            {
                title = String.Empty,
                description = String.Empty,
                date = _selectDate,
                index = 0,
                IsAdd = true,
                viewModel = _viewModel
            };
            GUIManager.Instance.ShowWindow(param);
        }

        public void BeforeShow()
        {
            // 初始化三个月队列
            _currentMonth = DateTime.UtcNow.Month;
            _currentYear = DateTime.UtcNow.Year;
            m_IsShowDay.selectedIndex = 0;
            UpdateMonthInfo();

            months.Clear();
            months.Enqueue(GetMonth(_currentMonth - 1));
            months.Enqueue(_currentMonth);
            months.Enqueue(GetMonth(_currentMonth + 1));

            UpdateView(); // 初始刷新
        }
        private void UpdateMonthInfo()
        {
            DateTime firstDay = new DateTime(_currentYear, _currentMonth, 1);
            _notCurrent = (int)firstDay.DayOfWeek;
            _daysInMonth = DateTime.DaysInMonth(_currentYear, _currentMonth);
            _prevMonthDays = DateTime.DaysInMonth(_currentYear, GetMonth(_currentMonth - 1));
            UpdateMonthText();
        }

        private int GetMonth(int month)
        {
            if (month > 12) return month - 12;
            if (month < 1) return month + 12;
            return month;
        }

        private void UpdateView()
        {
            UpdateMonthInfo();
            m_calendar.numItems = 42; // 更新日历网格
            UpdateDetailView(_selectDate);
        }

        private void OnRenderItem(int index, GObject item)
        {
            var dayItem = item as UI_CalendarDayCom;
            dayItem.Init();
            int dayToShow;

            if (index < _notCurrent) // 上个月
            {
                dayToShow = _prevMonthDays - _notCurrent + index + 1;
                dayItem.Update(dayToShow, false, null);
                dayItem.onClick.Clear();
            }
            else if (index < _notCurrent + _daysInMonth) // 本月
            {
                dayToShow = index - _notCurrent + 1;
                var date = new DateTime(_currentYear, _currentMonth, dayToShow);
                var tasks = _viewModel.GetTasksForDay(date);
                dayItem.Update(dayToShow, true, tasks);
                dayItem.onClick.Set(() => OnClickDayBtn(date));
            }
            else // 下个月
            {
                dayToShow = index - _notCurrent - _daysInMonth + 1;
                dayItem.Update(dayToShow, false, null);
                dayItem.onClick.Clear();
            }
        }

        private void OnRenderTaskItem(int index, GObject item)
        {
            var taskItem = item as UI_detailTaskCom;
            taskItem.viewModel = _viewModel;
            var tasks = _viewModel.GetTasksForDay(_selectDate);
            taskItem.UpdateView(_selectDate, index, tasks);
        }

        private void OnClickDayBtn(DateTime date)
        {
            _selectDate = date;
            m_IsShowDay.selectedIndex = 1;
            UpdateDetailView(_selectDate);
        }

        private void UpdateDetailView(DateTime selectDay)
        {
            //每次刷新都重置按钮
            m_addBtn.selected = false;
            _selectDate = selectDay;
            m_dateTxt.text = selectDay.Day.ToString();
            var tasks = _viewModel.GetTasksForDay(selectDay);
            m_taskList.numItems = tasks.Count;
        }

        private void ShowNextMonth()
        {
            _currentMonth++;
            if (_currentMonth > 12)
            {
                _currentMonth = 1;
                _currentYear++;
            }
            months.Dequeue();
            months.Enqueue(GetMonth(_currentMonth + 1));
            UpdateView();
        }

        private void ShowPrevMonth()
        {
            _currentMonth--;
            if (_currentMonth < 1)
            {
                _currentMonth = 12;
                _currentYear--;
            }
            months.Dequeue();
            months.Enqueue(GetMonth(_currentMonth - 1));
            UpdateView();
        }

        private void FoldDetail()
        {
            m_IsShowDay.selectedIndex = 0;
        }

        private void CanEdit()
        {
            UpdateDetailView(_selectDate);
        }

        private void UpdateMonthText()
        {
            m_month.text = $"{_currentMonth}月";
        }

        public void OnHide()
        {
            EventManager.Instance.RemoveEvent(ClientEvent.UPDATE_CALENDAR_VIEW, UpdateView);
        }
    }
}