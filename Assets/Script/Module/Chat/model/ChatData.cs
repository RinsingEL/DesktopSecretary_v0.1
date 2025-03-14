using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;

public class ChatData
{
    // 设置数据
    public string ApiKey;

    // 使用 List 保存聊天记录，保证顺序
    public List<ChatMessage> History = new List<ChatMessage>();

    // 表示单条聊天消息的类
    [System.Serializable]
    public class ChatMessage
    {
        public string Role;
        public string Content;
        public long Timestamp; // 可选：记录消息时间戳

        public ChatMessage(string role, string content)
        {
            Role = role;
            Content = content;
            Timestamp = System.DateTimeOffset.Now.ToUnixTimeSeconds(); // 当前时间戳
        }
    }

    // 添加消息的辅助方法
    public void Add(string role, string content)
    {
        History.Add(new ChatMessage(role, content));
        SaveToLocal(); // 每次添加时保存
    }

    // 保存到本地
    public void SaveToLocal()
    {
        string json = JsonConvert.SerializeObject(this, Formatting.Indented);
        string path = Path.Combine(Application.persistentDataPath, "ChatHistory.json");
        File.WriteAllText(path, json);
        Debug.Log("Chat history saved to: " + path);
    }

    // 从本地加载
    public static ChatData LoadFromLocal()
    {
        string path = Path.Combine(Application.persistentDataPath, "ChatHistory.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ChatData>(json);
        }
        return new ChatData();
    }
}