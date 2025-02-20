// ConfigManager.cs
using UnityEngine;

namespace Core.Framework.Config
{
    public class ConfigManager
    {
        // ����ģʽ
        private static ConfigManager instance;
        public static ConfigManager Instance => instance ??= new ConfigManager();

        // ���ù�����
        public NetworkConfigManager Network { get; } = new NetworkConfigManager();
        public UIConfigManager UI { get; } = new UIConfigManager();
        public UserSettingManager Game { get; } = new UserSettingManager();

        // ��ʼ������
        public void Initialize()
        {
            LoadAllConfigs();
        }

        // ������������
        private void LoadAllConfigs()
        {
            Network.Load();
            UI.Load();
            Game.Load();
        }

        // ������������
        public void SaveAllConfigs()
        {
            Network.Save();
            UI.Save();
            Game.Save();
        }
    }

    // UIConfigManager.cs
    public class UIConfigManager
    {
        public string DefaultFont { get; private set; } = "Arial";
        public Color ThemeColor { get; private set; } = Color.white;
        public float UIScale { get; private set; } = 1.0f;

        public void Load()
        {
            // �ӳ־û��洢����UI����
            DefaultFont = PlayerPrefs.GetString("UI_DefaultFont", DefaultFont);
            ThemeColor = LoadColor("UI_ThemeColor", ThemeColor);
            UIScale = PlayerPrefs.GetFloat("UI_Scale", UIScale);
        }

        public void Save()
        {
            // ����UI���õ��־û��洢
            PlayerPrefs.SetString("UI_DefaultFont", DefaultFont);
            SaveColor("UI_ThemeColor", ThemeColor);
            PlayerPrefs.SetFloat("UI_Scale", UIScale);
        }

        private Color LoadColor(string key, Color defaultColor)
        {
            if (PlayerPrefs.HasKey(key))
            {
                string colorHex = PlayerPrefs.GetString(key);
                if (ColorUtility.TryParseHtmlString(colorHex, out Color color))
                {
                    return color;
                }
            }
            return defaultColor;
        }

        private void SaveColor(string key, Color color)
        {
            string colorHex = ColorUtility.ToHtmlStringRGBA(color);
            PlayerPrefs.SetString(key, $"#{colorHex}");
        }
    }

    // GameConfigManager.cs
    public class UserSettingManager
    {
        public string Language { get; private set; } = "zh-CN";
        public float MasterVolume { get; private set; } = 1.0f;

        public void Load()
        {
            // �ӳ־û��洢������Ϸ����
            Language = PlayerPrefs.GetString("Game_Language", Language);
            MasterVolume = PlayerPrefs.GetFloat("Game_MasterVolume", MasterVolume);
        }

        public void Save()
        {
            // ������Ϸ���õ��־û��洢
            PlayerPrefs.SetString("Game_Language", Language);
            PlayerPrefs.SetFloat("Game_MasterVolume", MasterVolume);
        }
    }
}