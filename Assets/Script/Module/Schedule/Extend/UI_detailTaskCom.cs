using FairyGUI.Utils;
using FairyGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Framework.Resource;
using System;
using Core.Framework.FGUI;

namespace Com.Module.Schedule
{
    public partial class UI_detailTaskCom : GComponent
    {
        DateTime date;
        public void UpdateView(DateTime day, int index, List<DBClass.Task> tasks , bool IsCanEdit)
        {
            date = day;
            m_titleTxt.text = tasks[index].Title;
            m_taskDesTxt.text = tasks[index].Description;
            if (IsCanEdit)
            {
                m_taskBtn.touchable = true;
                m_taskBtn.onClick.Set(EditTask);
            }
            else
            {
                m_taskBtn.onClick.Clear();
                m_taskBtn.touchable = false;
            }

        }

        private void EditTask(EventContext context)
        {
            var param = new EditWindow.EditWindowParam()
            {
                title = m_titleTxt.text,
                description = m_taskDesTxt.text,
                date = this.date,
            };
            GUIManager.Instance.ShowWindow(param);
        }
    }
}
