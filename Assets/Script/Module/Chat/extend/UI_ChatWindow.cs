using FairyGUI.Utils;
using FairyGUI;
using System;
using Core.Framework.Event;
using Core.Framework.Config;
using Core.Framework.FGUI;

namespace Com.Module.Chat
{
    public partial class UI_ChatWindow : GComponent
    {
        public void BeforeShow()
        {
            m_inputTxt.text = " ";
            m_inputTxt.onSubmit.Set(OnEnterInput);
        }
        private void OnEnterInput(EventContext context)
        {
            if (m_inputTxt.text != " ")
            {
                EventManager.Instance.Trigger<string, string, string>(ClientEvent.ON_SEND_FUNC_REQUEST, ConfigManager.Instance.Game.Prompt, m_inputTxt.text, null);
                m_inputTxt.text = " ";
            }
        }

        public void OnHide()
        {
            m_inputTxt.onSubmit.Clear();
        }
    }
}