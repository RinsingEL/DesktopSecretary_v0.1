using UnityEngine;
using FairyGUI;
using Com.Module.CommonResources;
using Core.Framework.Config;

namespace Com.Module.Chat
{
    public partial class UI_SettingPanel : GComponent
    {
        bool IsScaleChanged = false;
        bool IsXChanged = false;
        bool IsYChanged = false;
        public void Init()
        {
            ((UI_frameClose)m_frame).m_title.text = "ÉèÖÃ";
            m_inputTxt1.text = ConfigManager.Instance.Network.apiKey;
            m_inputTxt2.text = ConfigManager.Instance.Game.Prompt;
            m_sizeSlide.value = ConfigManager.Instance.UI.UIScale * 20;
            m_XSlide.value = ConfigManager.Instance.UI.HorizontalOffset / 7.8f * 50 + 50;
            m_YSlide.value = ConfigManager.Instance.UI.VerticalOffset / 3f * 50 + 50;
            m_sizeSlide.onChanged.Set(() => {
                if (m_sizeSlide.value > 0.2)
                    Pet.Instance.transform.localScale = new Vector3((float)m_sizeSlide.value/20, (float)m_sizeSlide.value/20, (float)m_sizeSlide.value / 20);
                else
                    Pet.Instance.transform.localScale = Vector3.one;
                IsScaleChanged = true;
            });
            m_XSlide.onChanged.Set(() => {
                Pet.Instance.transform.position = (float)m_XSlide.value > 50 ?
                new Vector3(((float)m_XSlide.value - 50) / 50 * 7.8f, Pet.Instance.transform.position.y,  Pet.Instance.transform.position.z) :
                new Vector3(((float)m_XSlide.value - 50) / 50 * 7.8f, Pet.Instance.transform.position.y, Pet.Instance.transform.position.z);
                IsXChanged = true;
            });
            m_YSlide.onChanged.Set(() => {
                Pet.Instance.transform.position = (float)m_YSlide.value > 50? 
                new Vector3(Pet.Instance.transform.position.x, ((float)m_YSlide.value - 50) / 50 * 3f, Pet.Instance.transform.position.z):
                new Vector3(Pet.Instance.transform.position.x, ((float)m_YSlide.value - 50) / 50 * 3f, Pet.Instance.transform.position.z);
                IsYChanged = true;
            });
            m_saveBtn.onClick.Set(SaveSetting);
            m_ResetBtn.onClick.Set(ResetSetting);
        }
        public void SaveSetting()
        {
            if (m_inputTxt1.text != string.Empty)
                ConfigManager.Instance.Network.apiKey = m_inputTxt1.text;
            if(m_inputTxt2.text != string.Empty)
                ConfigManager.Instance.Game.Prompt = m_inputTxt2.text;
            if(IsScaleChanged)
                ConfigManager.Instance.UI.UIScale = (float)m_sizeSlide.value / 20;
            if (IsXChanged)
                ConfigManager.Instance.UI.HorizontalOffset = ((float)m_XSlide.value - 50) / 50 * 7.8f;
            if(IsYChanged)
                ConfigManager.Instance.UI.VerticalOffset = ((float)m_YSlide.value - 50) / 50 * 3f;
            ConfigManager.Instance.Network.Save();
            ConfigManager.Instance.UI.Save();
            ConfigManager.Instance.Game.Save();
        }
        public void ResetSetting(EventContext context)
        {
            ConfigManager.Instance.Network.apiKey = string.Empty;
            m_inputTxt1.text = string.Empty;
            m_inputTxt2.text = string.Empty;
            m_sizeSlide.value = 0;
            m_XSlide.value = 0;
            m_YSlide.value = 0;
        }
        public void OnHide()
        {
            m_sizeSlide.onChanged.Clear();
            m_XSlide.onChanged.Clear();
            m_YSlide.onChanged.Clear();
        }
    }
}