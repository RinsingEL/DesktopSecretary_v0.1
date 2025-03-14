using Com.Module.Schedule;
using Core.Framework.FGUI;
using FairyGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Module.Schedule
{
    public class EditWindow : GUIWindow
    {
        UI_EditWindow rootWindow;
        EditWindowParam param;
        public EditWindow()
        {
            Param.packagePath = "UI/Schedule";
            Param.packageName = "Schedule";
            Param.componentName = "EditWindow";
            Param.Layer = UILayer.Popup;
        }
        protected override void OnInit(GComponent com)
        {
            rootWindow = com as UI_EditWindow;
            rootWindow.Init(param);
            rootWindow.m_cancelBtn.onClick.Add(Destroy);//感觉也能做成绑个事件让他修改事件后Hide
            rootWindow.m_saveBtn.onClick.Add(Destroy);
        }
        public override void InitializeParam(ShowWindowParam param)
        {
            this.param = param as EditWindowParam;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            rootWindow.m_cancelBtn.onClick.Remove(Destroy);
            rootWindow.m_saveBtn.onClick.Remove(Destroy);
        }
        public class EditWindowParam : ShowWindowParam
        {
            public string title;
            public string description;
            public int index;
            public DateTime date;
            public bool IsAdd = false;
            public CalendarViewModel viewModel;
        }
    }
}

