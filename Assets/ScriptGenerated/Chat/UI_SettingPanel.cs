/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Com.Module.Chat
{
    public partial class UI_SettingPanel : GComponent
    {
        public GTextInput m_inputTxt1;
        public GComponent m_frame;
        public GButton m_saveBtn;
        public GButton m_ResetBtn;
        public const string URL = "ui://4kme1nf9jnhn3";

        public static UI_SettingPanel CreateInstance()
        {
            return (UI_SettingPanel)UIPackage.CreateObject("Chat", "SettingPanel");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_inputTxt1 = (GTextInput)GetChild("inputTxt1");
            m_frame = (GComponent)GetChild("frame");
            m_saveBtn = (GButton)GetChild("saveBtn");
            m_ResetBtn = (GButton)GetChild("ResetBtn");
        }
    }
}