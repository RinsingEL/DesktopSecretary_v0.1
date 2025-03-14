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
        public CalendarViewModel viewModel;
        public void Init(CalendarViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public void UpdateView(DateTime day, int index, List<DBClass.Task> tasks)
        {
            date = day;
            m_titleTxt.text = tasks[index].Title;
            m_taskDesTxt.text = tasks[index].Description;
            m_taskBtn.touchable = true;
            m_taskBtn.onClick.Set(() =>
            {
                var param = new EditWindow.EditWindowParam()
                {
                    title = m_titleTxt.text,
                    description = m_taskDesTxt.text,
                    date = this.date,
                    index = index,
                    viewModel = this.viewModel
                };
                GUIManager.Instance.ShowWindow(param);
            });
        }

        private void EditTask(EventContext context)
        {

        }
        private void AddTask()
        {
            var param = new EditWindow.EditWindowParam()
            {
                title = m_titleTxt.text,
                description = m_taskDesTxt.text,
                date = this.date,
                viewModel = this.viewModel
            };
            GUIManager.Instance.ShowWindow(param);
        }
    }
}
