/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Com.Module.Schedule
{
    public partial class UI_EditWindow : GComponent
    {
        public GTextField m_titleTxt;
        public GTextInput m_titleInput;
        public GTextField m_desTxt;
        public GTextInput m_desInput;
        public GButton m_saveBtn;
        public GButton m_cancelBtn;
        public const string URL = "ui://msqew0pqgbhr7";

        public static UI_EditWindow CreateInstance()
        {
            return (UI_EditWindow)UIPackage.CreateObject("Schedule", "EditWindow");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_titleTxt = (GTextField)GetChild("titleTxt");
            m_titleInput = (GTextInput)GetChild("titleInput");
            m_desTxt = (GTextField)GetChild("desTxt");
            m_desInput = (GTextInput)GetChild("desInput");
            m_saveBtn = (GButton)GetChild("saveBtn");
            m_cancelBtn = (GButton)GetChild("cancelBtn");
        }
    }
}