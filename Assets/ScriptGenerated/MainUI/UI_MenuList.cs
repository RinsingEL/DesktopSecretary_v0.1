/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Com.Module.MainUI
{
    public partial class UI_MenuList : GComponent
    {
        public GList m_MenuList;
        public const string URL = "ui://fq000rj2e53x1";

        public static UI_MenuList CreateInstance()
        {
            return (UI_MenuList)UIPackage.CreateObject("MainUI", "MenuList");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_MenuList = (GList)GetChild("MenuList");
        }
    }
}