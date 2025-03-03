/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Com.Module.Schedule
{
    public partial class UI_CalendarDayTaskCom : GComponent
    {
        public GGraph m_taskColor;
        public GTextField m_taskTitle;
        public const string URL = "ui://msqew0pqdipb2";

        public static UI_CalendarDayTaskCom CreateInstance()
        {
            return (UI_CalendarDayTaskCom)UIPackage.CreateObject("Schedule", "CalendarDayTaskCom");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_taskColor = (GGraph)GetChild("taskColor");
            m_taskTitle = (GTextField)GetChild("taskTitle");
        }
    }
}