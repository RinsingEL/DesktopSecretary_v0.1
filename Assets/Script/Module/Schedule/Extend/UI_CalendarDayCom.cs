using FairyGUI.Utils;
using FairyGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Core.Framework.Resource;

namespace Com.Module.Schedule
{
    public partial class UI_CalendarDayCom : GComponent
    {
        public List<DBClass.Task> tasks;
        public void Init()
        {
            m_taskLIst.itemRenderer = OnRenderItem;
        }

        public void Update(int day , bool IsCurrent , List<DBClass.Task> tasks)
        {
            this.tasks = tasks;
            m_day.text = day.ToString();
            if (IsCurrent)
            {
                m_c1.selectedIndex = 0;
                if (tasks != null)
                    m_taskLIst.numItems = tasks.Count;
                else
                    m_taskLIst.numItems = 0;
            }
            else
            {
                m_c1.selectedIndex = 1;
                m_taskLIst.numItems = 0;
            }
                

        }

        private void OnRenderItem(int index, GObject item)
        {
            var taskItem = item as UI_CalendarDayTaskCom;
            taskItem.m_taskTitle.text = tasks[index].Title;
        }
    }
}
