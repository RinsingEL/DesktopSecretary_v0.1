using Com.Module.CommonResources;
using Core.Framework.FGUI;
using FairyGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Com.Module.Chat
{
    public class SettingWindow : GUIWindow
    {
        public UI_SettingPanel rootwindow;

        public SettingWindow()
        {
            Param.packageName = "Chat";
            Param.componentName = "SettingPanel";
            Param.packagePath = "UI/Chat";
            Param.Layer = UILayer.Normal;
        }
        protected override void OnInit(GComponent com)
        {

            rootwindow = com as UI_SettingPanel;
            rootwindow.Init();
            ((UI_frameClose)rootwindow.m_frame).m_closeBtn.onClick.Set(Hide);
        }
    }
}