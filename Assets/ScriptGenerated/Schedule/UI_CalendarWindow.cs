/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Com.Module.Schedule
{
    public partial class UI_CalendarWindow : GComponent
    {
        public Controller m_IsDetail;
        public Controller m_IsShowDay;
        public GGraph m_bgr;
        public GList m_calendar;
        public GTextField m_month;
        public GButton m_prevBtn;
        public GButton m_nextBtn;
        public GButton m_todayBtn;
        public GButton m_closeBtn;
        public GGraph m_bgl;
        public GTextField m_dateTxt;
        public GList m_taskList;
        public GButton m_detailCollapseBtn;
        public GButton m_addBtn;
        public GGroup m_left;
        public const string URL = "ui://msqew0pqsiyd0";

        public static UI_CalendarWindow CreateInstance()
        {
            return (UI_CalendarWindow)UIPackage.CreateObject("Schedule", "CalendarWindow");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_IsDetail = GetController("IsDetail");
            m_IsShowDay = GetController("IsShowDay");
            m_bgr = (GGraph)GetChild("bgr");
            m_calendar = (GList)GetChild("calendar");
            m_month = (GTextField)GetChild("month");
            m_prevBtn = (GButton)GetChild("prevBtn");
            m_nextBtn = (GButton)GetChild("nextBtn");
            m_todayBtn = (GButton)GetChild("todayBtn");
            m_closeBtn = (GButton)GetChild("closeBtn");
            m_bgl = (GGraph)GetChild("bgl");
            m_dateTxt = (GTextField)GetChild("dateTxt");
            m_taskList = (GList)GetChild("taskList");
            m_detailCollapseBtn = (GButton)GetChild("detailCollapseBtn");
            m_addBtn = (GButton)GetChild("addBtn");
            m_left = (GGroup)GetChild("left");
        }
    }
}