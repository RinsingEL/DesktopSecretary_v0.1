using Com.Module.Chat;
using Com.Module.Schedule;
using Core.Framework.FGUI;
using FairyGUI;
using System;
using UnityEngine;
using Com.Module.Watcher;
using System.Runtime.InteropServices;

namespace Com.Module.MainUI
{
    public partial class UI_MenuList : GComponent
    {
        public void Init()
        {
            //Stage.inst.onKeyDown.Set(OnKeyDown);
            m_MenuList.itemRenderer = RenderListItem;
            m_MenuList.numItems = 3;
        }

        public void RenderListItem(int index, GObject Menubtn)
        {
            switch(index)
            {
                case 0:
                    Menubtn.asButton.onClick.Set(OpenSettingPanel);
                    break;
                case 1:
                    Menubtn.asButton.onClick.Set(OpenCalendarPanel);
                    break;
                case 2:
                    Menubtn.asButton.onClick.Set(TestFocusCheck);
                    Menubtn.asButton.text = "专注测试";
                    break;
                default:
                    break;
            }
        }

        private void OpenCalendarPanel(EventContext context)
        {
            //记得在这打开窗口
            GUIManager.Instance.ShowWindow<CalendarWindow>();
        }

        public void OpenSettingPanel(EventContext context)
        {
            GUIManager.Instance.ShowWindow<SettingWindow>();
        }

        private void TestFocusCheck(EventContext context)
        {
            // 设置测试任务
            WatcherPlugin.Instance.SetCurrentTask("测试专注度检测功能");
            
            // 手动触发一次专注检测
            string windowTitle = GetActiveWindowTitleForTest();
            WatcherPlugin.Instance.ShowReminder($"测试提醒：\n当前窗口：{windowTitle}\n请保持专注！");
        }

        private string GetActiveWindowTitleForTest()
        {
            try 
            {
                const int nChars = 256;
                System.Text.StringBuilder buff = new System.Text.StringBuilder(nChars);
                IntPtr handle = GetForegroundWindow();
                if (GetWindowText(handle, buff, nChars) > 0)
                {
                    return buff.ToString();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"获取窗口标题失败：{e.Message}");
            }
            return "测试窗口标题";
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);
    }
}

