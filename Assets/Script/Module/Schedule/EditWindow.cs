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
            rootWindow.m_cancelBtn.onClick.Set(Hide);
        }
        public override void InitializeParam(ShowWindowParam param)
        {
            this.param = param as EditWindowParam;
        }
        public class EditWindowParam : ShowWindowParam
        {
            public string title;
            public string description;
            public int index;
            public DateTime date;
        }
    }
}

