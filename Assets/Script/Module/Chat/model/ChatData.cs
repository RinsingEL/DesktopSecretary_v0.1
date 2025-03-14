using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;

public class ChatData
{
    // ��������
    public string ApiKey;

    // ʹ�� List ���������¼����֤˳��
    public List<ChatMessage> History = new List<ChatMessage>();

    // ��ʾ����������Ϣ����
    [System.Serializable]
    public class ChatMessage
    {
        public string Role;
        public string Content;
        public long Timestamp; // ��ѡ����¼��Ϣʱ���

        public ChatMessage(string role, string content)
        {
            Role = role;
            Content = content;
            Timestamp = System.DateTimeOffset.Now.ToUnixTimeSeconds(); // ��ǰʱ���
        }
    }

    // �����Ϣ�ĸ�������
    public void Add(string role, string content)
    {
        History.Add(new ChatMessage(role, content));
        SaveToLocal(); // ÿ�����ʱ����
    }

    // ���浽����
    public void SaveToLocal()
    {
        string json = JsonConvert.SerializeObject(this, Formatting.Indented);
        string path = Path.Combine(Application.persistentDataPath, "ChatHistory.json");
        File.WriteAllText(path, json);
        Debug.Log("Chat history saved to: " + path);
    }

    // �ӱ��ؼ���
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