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

        // ���ù�����
        public NetworkConfigManager Network { get; } = new NetworkConfigManager();
        public UIConfigManager UI { get; } = new UIConfigManager();
        public UserSettingManager Game { get; } = new UserSettingManager();

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

    public class UIConfigManager
    {
        public float UIScale = 1.0f;
        public float HorizontalOffset = 0f; // ��ƫ����
        public float VerticalOffset = 0f;   // ��ƫ����

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

        // �����Զ�̬�жϴ�û��
        public bool IsFirstTimeOpen
        {
            get
            {
                // ��� PlayerPrefs ��û���������˵���ǵ�һ�δ�
                return !PlayerPrefs.HasKey("Game_IsFirstTimeOpen");
            }
        }

        public void Load()
        {
            Prompt = PlayerPrefs.GetString("Game_Prompt", Prompt);

            // ����ǵ�һ�δ򿪣���ʼ��ʱ����һ����־
            if (IsFirstTimeOpen)
            {
                PlayerPrefs.SetInt("Game_IsFirstTimeOpen", 0); // 0 ��ʾ�Ѵ򿪹�
                PlayerPrefs.Save(); // ��������
            }
        }

        public void Save()
        {
            PlayerPrefs.SetString("Game_Prompt", Prompt);
            PlayerPrefs.Save();
        }
    }
}