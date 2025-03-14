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
        public GTextInput m_inputTxt2;
        public GSlider m_sizeSlide;
        public GSlider m_YSlide;
        public GSlider m_XSlide;
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
            m_inputTxt2 = (GTextInput)GetChild("inputTxt2");
            m_sizeSlide = (GSlider)GetChild("sizeSlide");
            m_YSlide = (GSlider)GetChild("YSlide");
            m_XSlide = (GSlider)GetChild("XSlide");
        }
    }
}