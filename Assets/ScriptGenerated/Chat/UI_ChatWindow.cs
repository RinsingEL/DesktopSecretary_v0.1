/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Com.Module.Chat
{
    public partial class UI_ChatWindow : GComponent
    {
        public GTextInput m_inputTxt;
        public GButton m_btn_history;
        public const string URL = "ui://4kme1nf9k5oi6";

        public static UI_ChatWindow CreateInstance()
        {
            return (UI_ChatWindow)UIPackage.CreateObject("Chat", "ChatWindow");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_inputTxt = (GTextInput)GetChild("inputTxt");
            m_btn_history = (GButton)GetChild("btn_history");
        }
    }
}