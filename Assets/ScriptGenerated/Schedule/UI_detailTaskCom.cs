/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Com.Module.Schedule
{
    public partial class UI_detailTaskCom : GComponent
    {
        public GGraph m_titleBg;
        public GTextField m_titleTxt;
        public GTextField m_taskDesTxt;
        public const string URL = "ui://msqew0pqdipb6";

        public static UI_detailTaskCom CreateInstance()
        {
            return (UI_detailTaskCom)UIPackage.CreateObject("Schedule", "detailTaskCom");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_titleBg = (GGraph)GetChild("titleBg");
            m_titleTxt = (GTextField)GetChild("titleTxt");
            m_taskDesTxt = (GTextField)GetChild("taskDesTxt");
        }
    }
}