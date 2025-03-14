using Core.Framework.FGUI;
using FairyGUI;
using Module.chat;

namespace Com.Module.Chat
{
    public class ChatWindow : GUIWindow
    {
        UI_ChatWindow rootWindow;
        public ChatWindow()
        {
            Param.packagePath = "UI/Chat";
            Param.packageName = "Chat";
            Param.componentName = "ChatWindow";
            Param.Layer = UILayer.Normal;
        }

        protected override void OnInit(GComponent com)
        {
            rootWindow = com as UI_ChatWindow;
            rootWindow.m_btn_history.onClick.Set(ShowHistory);
        }
        protected override void BeforeShow()
        {
            base.BeforeShow();
            rootWindow.BeforeShow();
        }

        protected override void OnHide()
        {
            base.OnHide();
            rootWindow.OnHide();
        }
        public void ShowHistory()
        {
            Hide();
            HistoryPanel.HistoryParam historyParam = new HistoryPanel.HistoryParam
            {
                History = ChatPlugin.Instance.GetChatHistory() // 从 ChatPlugin 获取数据
            };
            GUIManager.Instance.ShowWindow(historyParam);
        }
    }
}