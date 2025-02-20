using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using FairyGUI.Utils;
using Com.Module.CommonResources;
using Module.chat;
using Core.Framework.Config;

namespace Com.Module.Chat
{
    public partial class UI_SettingPanel : GComponent
    {
        public void Init()
        {
            ((UI_frameClose)m_frame).m_title.text = "…Ë÷√";
            m_inputTxt1.text = ConfigManager.Instance.Network.apiKey;
            m_saveBtn.onClick.Set(SaveSetting);
            m_ResetBtn.onClick.Set(ResetSetting);
        }
        public void SaveSetting()
        {
            if (m_inputTxt1.text != string.Empty)
            {
                ConfigManager.Instance.Network.apiKey = m_inputTxt1.text;
                m_inputTxt1.text = ConfigManager.Instance.Network.apiKey;
                ConfigManager.Instance.Network.Save();
            }

        }
        public void ResetSetting(EventContext context)
        {
            ChatPlugin.Instance.SendChatMessage("ƒ„∫√");
            /*            ConfigManager.Instance.Network.apiKey = string.Empty;
                        m_inputTxt1.text = string.Empty;*/
        }
    }
}