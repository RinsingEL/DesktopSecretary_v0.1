/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Com.Module.Watcher
{
    public partial class UI_WatcherWindow : GComponent
    {
        public Controller m_IsFold;
        public GGraph m_bg;
        public GImage m_clock;
        public GTextField m_taskTitle;
        public GTextField m_taskDes;
        public GButton m_startBtn;
        public GButton m_resetBtn;
        public GButton m_FinishBtn;
        public GTextField m_clockTime;
        public GGroup m_all;
        public GButton m_folderBtn;
        public const string URL = "ui://jshw9ftji9oh0";

        public static UI_WatcherWindow CreateInstance()
        {
            return (UI_WatcherWindow)UIPackage.CreateObject("Watcher", "WatcherWindow");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_IsFold = GetController("IsFold");
            m_bg = (GGraph)GetChild("bg");
            m_clock = (GImage)GetChild("clock");
            m_taskTitle = (GTextField)GetChild("taskTitle");
            m_taskDes = (GTextField)GetChild("taskDes");
            m_startBtn = (GButton)GetChild("startBtn");
            m_resetBtn = (GButton)GetChild("resetBtn");
            m_FinishBtn = (GButton)GetChild("FinishBtn");
            m_clockTime = (GTextField)GetChild("clockTime");
            m_all = (GGroup)GetChild("all");
            m_folderBtn = (GButton)GetChild("folderBtn");
        }
    }
}