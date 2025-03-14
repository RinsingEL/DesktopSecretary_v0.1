// ConfigManager.cs
using Core.Framework.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace Core.Framework.Config
{
    public class ConfigManager
    {
        private static ConfigManager instance;
        public static ConfigManager Instance => instance ??= new ConfigManager();

        // 配置管理器
        public NetworkConfigManager Network { get; } = new NetworkConfigManager();
        public UIConfigManager UI { get; } = new UIConfigManager();
        public UserSettingManager Game { get; } = new UserSettingManager();

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

    public class UIConfigManager
    {
        public float UIScale = 1.0f;
        public float HorizontalOffset = 0f; // 横偏移量
        public float VerticalOffset = 0f;   // 竖偏移量

        public void Load()
        {
            UIScale = PlayerPrefs.GetFloat("UI_Scale", UIScale);
            HorizontalOffset = PlayerPrefs.GetFloat("UI_HorizontalOffset", HorizontalOffset);
            VerticalOffset = PlayerPrefs.GetFloat("UI_VerticalOffset", VerticalOffset);
        }

        public void Save()
        {
            PlayerPrefs.SetFloat("UI_Scale", UIScale);
            PlayerPrefs.SetFloat("UI_HorizontalOffset", HorizontalOffset);
            PlayerPrefs.SetFloat("UI_VerticalOffset", VerticalOffset);
        }
    }

    public class UserSettingManager
    {
        public string Prompt = "";

        // 用属性动态判断打开没有
        public bool IsFirstTimeOpen
        {
            get
            {
                // 如果 PlayerPrefs 中没有这个键，说明是第一次打开
                return !PlayerPrefs.HasKey("Game_IsFirstTimeOpen");
            }
        }

        public void Load()
        {
            Prompt = PlayerPrefs.GetString("Game_Prompt", Prompt);

            // 如果是第一次打开，初始化时保存一个标志
            if (IsFirstTimeOpen)
            {
                PlayerPrefs.SetInt("Game_IsFirstTimeOpen", 0); // 0 表示已打开过
                PlayerPrefs.Save(); // 立即保存
            }
        }

        public void Save()
        {
            PlayerPrefs.SetString("Game_Prompt", Prompt);
            PlayerPrefs.Save();
        }
    }
}