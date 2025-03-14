/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Com.Module.CommonResources
{
    public partial class UI_InputAndSelectCom : GComponent
    {
        public Controller m_c1;
        public GGraph m_bg;
        public GTextInput m_input;
        public GList m_selectList;
        public const string URL = "ui://agxl9rfpkmeh6";

        public static UI_InputAndSelectCom CreateInstance()
        {
            return (UI_InputAndSelectCom)UIPackage.CreateObject("CommonResources", "InputAndSelectCom");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_c1 = GetController("c1");
            m_bg = (GGraph)GetChild("bg");
            m_input = (GTextInput)GetChild("input");
            m_selectList = (GList)GetChild("selectList");
        }
    }
}