/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Com.Module.Chat
{
    public partial class UI_dialogCom : GComponent
    {
        public Controller m_from;
        public GTextField m_userTxt;
        public GLoader m_img;
        public GTextField m_AITxt;
        public const string URL = "ui://4kme1nf9hr1k8";

        public static UI_dialogCom CreateInstance()
        {
            return (UI_dialogCom)UIPackage.CreateObject("Chat", "dialogCom");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_from = GetController("from");
            m_userTxt = (GTextField)GetChild("userTxt");
            m_img = (GLoader)GetChild("img");
            m_AITxt = (GTextField)GetChild("AITxt");
        }
    }
}