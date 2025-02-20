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
            // 从持久化存储（如JSON文件或PlayerPrefs）加载网络配置
            apiKey = PlayerPrefs.GetString("Network_ApiKey", apiKey);
        }

        public void Save()
        {
            // 保存网络配置到持久化存储
            PlayerPrefs.SetString("Network_ApiKey", apiKey);
        }
    }
}