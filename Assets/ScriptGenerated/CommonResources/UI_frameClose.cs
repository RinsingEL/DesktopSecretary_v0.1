/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Com.Module.CommonResources
{
    public partial class UI_frameClose : GComponent
    {
        public GTextField m_title;
        public GButton m_closeBtn;
        public const string URL = "ui://agxl9rfpbjh30";

        public static UI_frameClose CreateInstance()
        {
            return (UI_frameClose)UIPackage.CreateObject("CommonResources", "frameClose");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_title = (GTextField)GetChild("title");
            m_closeBtn = (GButton)GetChild("closeBtn");
        }
    }
}