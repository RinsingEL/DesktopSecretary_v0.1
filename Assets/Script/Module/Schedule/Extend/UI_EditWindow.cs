using Core.Framework.FGUI;
using FairyGUI;
using Com.Module.CommonResources;
using static Com.Module.Schedule.EditWindow;
using System;

namespace Com.Module.Schedule
{
    public partial class UI_EditWindow : GComponent
    {
        private EditWindowParam _param;
        private int hour = 0;
        private int minute = 0;

        public void Init(EditWindowParam param)
        {
            _param = param;

            m_titleInput.text = param.title;
            m_titleInput.onFocusOut.Set(() => { _param.title = m_titleInput.text; });
            m_desInput.text = param.description;
            m_desInput.onFocusOut.Set(() => { _param.description = m_desInput.text; });
            ((UI_InputAndSelectCom)m_startinput).m_input.onClick.Set(() => {
                ((UI_InputAndSelectCom)m_startinput).m_c1.selectedIndex = 1;
                ((UI_InputAndSelectCom)m_startinput).m_selectList.selectedIndex = hour;
            });
            m_startinput.onFocusOut.Set(() => { 
                ((UI_InputAndSelectCom)m_startinput).m_c1.selectedIndex = 0;
                
            });
            ((UI_InputAndSelectCom)m_endInput).m_input.onClick.Set(() => { 
                ((UI_InputAndSelectCom)m_endInput).m_c1.selectedIndex = 1;
                ((UI_InputAndSelectCom)m_endInput).m_selectList.selectedIndex = hour;
            });
            m_endInput.onFocusOut.Set(() => {
                ((UI_InputAndSelectCom)m_endInput).m_c1.selectedIndex = 0; 
            });
            ((UI_InputAndSelectCom)m_startinput).m_selectList.itemRenderer = RenderInputTimeList;
            ((UI_InputAndSelectCom)m_endInput).m_selectList.itemRenderer = RenderEndTimeList;
            if (param.IsAdd)
            {
                m_saveBtn.title = "Ìí¼Ó";
                m_saveBtn.onClick.Add(AddTask);
            }
            else
            {
                m_saveBtn.title = "±£´æ";
                m_saveBtn.onClick.Set(SaveModify);
            }
            ((UI_InputAndSelectCom)m_startinput).m_selectList.numItems = 48;
            ((UI_InputAndSelectCom)m_endInput).m_selectList.numItems = 48;
        }

        private void RenderInputTimeList(int index, GObject item)
        {
            hour = index / 2;
            minute = index % 2 * 30;
            var time = String.Format("{0:D2}:{1:D2}",hour,minute);
            ((UI_SelectItem)item).m_content.text = time;
            item.onClick.Set(() => {
                ((UI_InputAndSelectCom)m_startinput).m_input.text = time;
                ((UI_InputAndSelectCom)m_startinput).m_c1.selectedIndex = 0;
            });
        }
        private void RenderEndTimeList(int index, GObject item)
        {
            hour = index / 2;
            minute = index % 2 * 30;
            var time = String.Format("{0:D2}:{1:D2}", hour, minute);
            ((UI_SelectItem)item).m_content.text = time;
            item.onClick.Set(() => { 
                ((UI_InputAndSelectCom)m_endInput).m_input.text = time;
                ((UI_InputAndSelectCom)m_endInput).m_c1.selectedIndex = 0;
            });
        }

        private void SaveModify()
        {
            _param.viewModel.ModifyTask(_param.date, _param.index, _param.title, _param.description);
        }

        private void AddTask()
        {
            TimeSpan StartSpan = TimeSpan.Parse(((UI_InputAndSelectCom)m_startinput).m_input.text);
            TimeSpan EndSpan = TimeSpan.Parse(((UI_InputAndSelectCom)m_startinput).m_input.text);
            var StartAt = _param.date.Add(StartSpan);
            var DueTime = _param.date.Add(EndSpan);
            _param.viewModel.AddNewTask(_param.date, _param.title, _param.description, StartAt, DueTime);
        }
    }
}