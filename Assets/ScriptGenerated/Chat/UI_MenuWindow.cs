/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Com.Module.Chat
{
    public partial class UI_MenuWindow : GComponent
    {
        public GButton m_btn_sendMsg;
        public const string URL = "ui://4kme1nf9jbiu0";

        public static UI_MenuWindow CreateInstance()
        {
            return (UI_MenuWindow)UIPackage.CreateObject("Chat", "MenuWindow");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_btn_sendMsg = (GButton)GetChild("btn_sendMsg");
        }
    }
}