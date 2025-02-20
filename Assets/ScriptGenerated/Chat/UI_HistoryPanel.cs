/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Com.Module.Chat
{
    public partial class UI_HistoryPanel : GComponent
    {
        public GList m_list_history;
        public GTextField m_titile;
        public const string URL = "ui://4kme1nf9d52d2";

        public static UI_HistoryPanel CreateInstance()
        {
            return (UI_HistoryPanel)UIPackage.CreateObject("Chat", "HistoryPanel");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_list_history = (GList)GetChild("list_history");
            m_titile = (GTextField)GetChild("titile");
        }
    }
}