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
        private int _currentMonth; // ��ǰ��ʾ���·�
        private int _currentYear;  // ��ǰ��ݣ����ڿ�����㣩
        private int _notCurrent;   // ���µ�һ�����ܼ���0=����, 1=��һ, ..., 6=������
        private int _daysInMonth;  // ��������
        private int _prevMonthDays; // �ϸ�������
        private CalendarData _calendarData;
        public Queue<int> months = new Queue<int>(3);
        public DateTime selectDate;
        bool _isCanEdit;

        public void Init()
        {
            _currentMonth = DateTime.UtcNow.Month;
            _currentYear = DateTime.UtcNow.Year;

            m_IsShowDay.selectedIndex = 0;
            UpdateMonthInfo(); // ��ʼ���·��������

            // ��ʼ��3���¶���
            months.Clear();
            months.Enqueue(GetMonth(_currentMonth - 1));
            months.Enqueue(_currentMonth);
            months.Enqueue(GetMonth(_currentMonth + 1));

            // �󶨰�ť�¼�����Ⱦ��
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

        // �����·������Ϣ
        private void UpdateMonthInfo()
        {
            DateTime firstDay = new DateTime(_currentYear, _currentMonth, 1);
            _notCurrent = (int)firstDay.DayOfWeek; // ���µ�һ�������ڼ�
            _daysInMonth = DateTime.DaysInMonth(_currentYear, _currentMonth); // ��������
            _prevMonthDays = DateTime.DaysInMonth(_currentYear, GetMonth(_currentMonth - 1)); // �ϸ�������
            UpdateMonthText();
        }

        // �����·�ѭ��
        private int GetMonth(int month)
        {
            if (month > 12) return month - 12;
            if (month < 1) return month + 12;
            return month;
        }

        // ������ͼ
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


        // ��Ⱦÿ�����ڸ���
        private void OnRenderItem(int index, GObject item)
        {
            var dayItem = item as UI_CalendarDayCom;
            int dayToShow;

            // 1. �ϸ��µ�β��
            if (index < _notCurrent)
            {
                dayToShow = _prevMonthDays - _notCurrent + index + 1;
                dayItem.Update(dayToShow, false, null); // false��ʾ�Ǳ���
                dayItem.onClick.Clear();
            }
            // 2. ��������
            else if (index < _notCurrent + _daysInMonth)
            {
                dayToShow = index - _notCurrent + 1;
                var date = new DateTime(_currentYear, _currentMonth, dayToShow).ToShortDateString();
                List<DBClass.Task> tasks;
                _calendarData.tasksByDay.TryGetValue(date, out tasks);
                dayItem.Update(dayToShow, true, tasks); // true��ʾ����
                dayItem.onClick.Set(OnClickDayBtn);
            }
            // 3. �¸��µĿ�ͷ
            else
            {
                dayToShow = index - _notCurrent - _daysInMonth + 1;
                dayItem.Update(dayToShow, false, null); // false��ʾ�Ǳ���
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

            // �л���������ͼ��ˢ��
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
                m_taskList.numItems = 0; // ���û��������ʾ���б�
            }
        }

        // ��ʾ�¸���
        public void ShowNextMonth(EventContext context)
        {
            _currentMonth++;
            if (_currentMonth > 12)
            {
                _currentMonth = 1;
                _currentYear++;
            }

            // ���¶���
            months.Dequeue();
            months.Enqueue(GetMonth(_currentMonth + 1));

            UpdateView(_calendarData);
            EventManager.Instance.Trigger<int>(ClientEvent.ON_MONTH_CHANGED, _currentMonth);
        }

        // ��ʾ�ϸ���
        public void ShowPrevMonth(EventContext context)
        {
            _currentMonth--;
            if (_currentMonth < 1)
            {
                _currentMonth = 12;
                _currentYear--;
            }

            // ���¶���
            months.Dequeue();
            months.Enqueue(GetMonth(_currentMonth - 1));

            UpdateView(_calendarData);
            EventManager.Instance.Trigger<int>(ClientEvent.ON_MONTH_CHANGED, _currentMonth);
        }

        // �����·���ʾ�ı�
        private void UpdateMonthText()
        {
            m_month.text = $"{_currentMonth}��";
        }

        // ʾ���������·ݱ仯
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