using Core.Framework.FGUI;
using FairyGUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Core.Framework.FGUI.GUIWindow;

namespace Com.Module.Watcher
{
    public class WatcherWindow : GUIWindow
    {
        private UI_WatcherWindow rootWindow;
        private WatcherWindowParam param;

        public WatcherWindow()
        {
            Param.packagePath = "UI/Watcher";
            Param.packageName = "Watcher";
            Param.componentName = "WatcherWindow";
            Param.Layer = UILayer.Popup;
        }

        protected override void OnInit(GComponent com)
        {
            rootWindow = com as UI_WatcherWindow;
            if (param != null)
            {
                rootWindow.Init(param);
            }
        }

        protected override void BeforeShow()
        {
            if (rootWindow != null && param != null)
            {
                rootWindow.Init(param);
            }
        }

        public override void InitializeParam(ShowWindowParam param)
        {
            base.InitializeParam(param);
            this.param = param as WatcherWindowParam;
            if (rootWindow != null)
            {
                rootWindow.Init(this.param);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        public class WatcherWindowParam : ShowWindowParam
        {
            public string title;           // 窗口标题
            public string description;     // 窗口描述
            public DateTime startTime;     // 开始时间
            public DateTime endTime;       // 结束时间
        }
    }

} 