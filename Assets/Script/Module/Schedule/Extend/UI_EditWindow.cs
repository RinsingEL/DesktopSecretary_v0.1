using Core.Framework.Event;
using Core.Framework.FGUI;
using FairyGUI;
using System;
using static Com.Module.Schedule.EditWindow;

namespace Com.Module.Schedule
{
    public partial class UI_EditWindow : GComponent
    {
        EditWindowParam param = new EditWindowParam();
        public void Init(EditWindowParam param)
        {
            this.param = param;
            m_titleInput.text = param.title;
            m_titleInput.onFocusOut.Set(() => { param.title = m_titleInput.text; });
            m_desInput.text = param.description;
            m_desInput.onFocusOut.Set(() => { param.description = m_desInput.text; });
            m_saveBtn.onClick.Set(SaveModify);
        }
        private void SaveModify()
        {
            SchedulePlugin.Instance.ModifyCalendarData(param.date, param.title, param.description, param.index);
            EventManager.Instance.Trigger(ClientEvent.UPDATE_CALENDAR_INFO);
            EventManager.Instance.Trigger(ClientEvent.UPDATE_CALENDAR_VIEW);
        }
        private void AddTask()
        {
            
        }
    }
}

