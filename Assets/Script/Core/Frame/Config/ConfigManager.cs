// ConfigManager.cs
using UnityEngine;

namespace Core.Framework.Config
{
    public class ConfigManager
    {
        // 单例模式
        private static ConfigManager instance;
        public static ConfigManager Instance => instance ??= new ConfigManager();

        // 配置管理器
        public NetworkConfigManager Network { get; } = new NetworkConfigManager();
        public UIConfigManager UI { get; } = new UIConfigManager();
        public UserSettingManager Game { get; } = new UserSettingManager();

        // 初始化方法
        public void Initialize()
        {
            LoadAllConfigs();
        }

        // 加载所有配置
        private void LoadAllConfigs()
        {
            Network.Load();
            UI.Load();
            Game.Load();
        }

        // 保存所有配置
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
            // 从持久化存储加载UI配置
            DefaultFont = PlayerPrefs.GetString("UI_DefaultFont", DefaultFont);
            ThemeColor = LoadColor("UI_ThemeColor", ThemeColor);
            UIScale = PlayerPrefs.GetFloat("UI_Scale", UIScale);
        }

        public void Save()
        {
            // 保存UI配置到持久化存储
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
            // 从持久化存储加载游戏配置
            Language = PlayerPrefs.GetString("Game_Language", Language);
            MasterVolume = PlayerPrefs.GetFloat("Game_MasterVolume", MasterVolume);
        }

        public void Save()
        {
            // 保存游戏配置到持久化存储
            PlayerPrefs.SetString("Game_Language", Language);
            PlayerPrefs.SetFloat("Game_MasterVolume", MasterVolume);
        }
    }
}