/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Com.Module.Chat
{
    public partial class UI_DialoguePanel : GComponent
    {
        public GTextField m_dialogueTxt;
        public const string URL = "ui://4kme1nf9k8kad";

        public static UI_DialoguePanel CreateInstance()
        {
            return (UI_DialoguePanel)UIPackage.CreateObject("Chat", "DialoguePanel");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_dialogueTxt = (GTextField)GetChild("dialogueTxt");
        }
    }
}