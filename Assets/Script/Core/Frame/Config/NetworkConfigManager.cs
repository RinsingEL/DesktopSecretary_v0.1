using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Core.Framework.Config
{
    public class NetworkConfigManager
    {
        public string apiKey = "xxxxxx";
        public string Model
        {
            get
            {
                if (model.Count != 0)
                    return model[0];
                return "";
            }
        }
        public List<string> model = new List<string>() { "gpt-3.5-turbo" };
        public void Load()
        {
            // �ӳ־û��洢����JSON�ļ���PlayerPrefs��������������
            apiKey = PlayerPrefs.GetString("Network_ApiKey", apiKey);
        }

        public void Save()
        {
            // �����������õ��־û��洢
            PlayerPrefs.SetString("Network_ApiKey", apiKey);
        }
    }
}