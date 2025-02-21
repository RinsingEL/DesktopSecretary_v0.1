/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Com.Module.Schedule
{
    public partial class UI_CalendarWindow : GComponent
    {
        public Controller m_IsDetail;
        public Controller m_IsShowDay;
        public GGraph m_bg;
        public GList m_calendar;
        public GTextField m_month;
        public GGraph m_bgl;
        public GTextField m_dateTxt;
        public GTextField m_descriptionTxt;
        public GList m_taskList;
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
            m_bg = (GGraph)GetChild("bg");
            m_calendar = (GList)GetChild("calendar");
            m_month = (GTextField)GetChild("month");
            m_bgl = (GGraph)GetChild("bgl");
            m_dateTxt = (GTextField)GetChild("dateTxt");
            m_descriptionTxt = (GTextField)GetChild("descriptionTxt");
            m_taskList = (GList)GetChild("taskList");
            m_left = (GGroup)GetChild("left");
        }
    }
}