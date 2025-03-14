/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Com.Module.CommonResources
{
    public partial class UI_SelectItem : GComponent
    {
        public GTextField m_content;
        public const string URL = "ui://agxl9rfpkmeh7";

        public static UI_SelectItem CreateInstance()
        {
            return (UI_SelectItem)UIPackage.CreateObject("CommonResources", "SelectItem");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_content = (GTextField)GetChild("content");
        }
    }
}