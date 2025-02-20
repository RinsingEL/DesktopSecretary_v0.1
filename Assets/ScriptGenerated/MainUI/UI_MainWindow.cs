/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Com.Module.MainUI
{
    public partial class UI_MainWindow : GComponent
    {
        public UI_MenuList m_MenuList;
        public const string URL = "ui://fq000rj2e53x2";

        public static UI_MainWindow CreateInstance()
        {
            return (UI_MainWindow)UIPackage.CreateObject("MainUI", "MainWindow");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_MenuList = (UI_MenuList)GetChild("MenuList");
        }
    }
}