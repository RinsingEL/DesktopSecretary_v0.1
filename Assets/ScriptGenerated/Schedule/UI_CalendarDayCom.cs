/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Com.Module.Schedule
{
    public partial class UI_CalendarDayCom : GComponent
    {
        public Controller m_c1;
        public GGraph m_bg;
        public GTextField m_day;
        public GList m_taskLIst;
        public const string URL = "ui://msqew0pqdipb1";

        public static UI_CalendarDayCom CreateInstance()
        {
            return (UI_CalendarDayCom)UIPackage.CreateObject("Schedule", "CalendarDayCom");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_c1 = GetController("c1");
            m_bg = (GGraph)GetChild("bg");
            m_day = (GTextField)GetChild("day");
            m_taskLIst = (GList)GetChild("taskLIst");
        }
    }
}