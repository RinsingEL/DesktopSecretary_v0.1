using Core.Framework.Config;
using Core.Framework.Network.ChatSystem;
using Core.Framework.Network;
using Module.chat;
using System;
using System.Collections.Generic;
using UnityEngine;
using Com.Module.Chat;
using Core.Framework.FGUI;
using Core.Framework.Utility;
using System.Runtime.InteropServices.ComTypes;

namespace Core.Framework.Pet
{
    [Serializable]
    public class Memory
    {
        public string content;        // 记忆内容
        public DateTime timestamp;    // 记忆时间戳
        public float importance;      // 记忆重要程度
    }

    public class PetAttributes : MonoBehaviour
    {
        // 短期记忆系统
        private List<Memory> shortTermMemories = new List<Memory>();
        public string importantMemories;
        private const int MAX_MEMORIES = 10;  // 最大记忆数量

        // 用户评价系统
        public float userTrustScore { get; private set; } = 50f;    // 信任度 (0-100)
        public float userAffectionScore { get; private set; } = 50f; // 亲密度 (0-100)
        public float userRespectScore { get; private set; } = 50f;   // 尊重度 (0-100)
        private void Start()
        {
            NetworkManager.Instance.AddEvent<string>(NetworkEvent.ON_CHAT_RESPONSE,OnNewMemoryResponse);
            RefreshMemory();
        }
        public void RefreshMemory()
        {
            long timestamp1 = DateTimeOffset.Now.ToUnixTimeSeconds(); // 第一个时间戳
            var history = ChatPlugin.Instance.GetChatHistory();
            long timestamp2 = history[ChatPlugin.Instance.GetChatHistory().Count - 1].Timestamp;

            // 将时间戳转换为 DateTimeOffset
            DateTimeOffset dateTime1 = DateTimeOffset.FromUnixTimeSeconds(timestamp1);
            DateTimeOffset dateTime2 = DateTimeOffset.FromUnixTimeSeconds(timestamp2);

            // 提取日期部分 (忽略时间)
            DateTime date1 = dateTime1.Date;
            DateTime date2 = dateTime2.Date;

            // 对比是否同一天
            bool isSameDay = date1 == date2;
            if (!isSameDay)
            {
                string lastDaydia = "";
                timestamp1 = history[history.Count - 1].Timestamp;
                for (int i = history.Count - 2; i >= 0; i--)
                {
                    timestamp2 = history[i].Timestamp;
                    // 将时间戳转换为 DateTimeOffset
                    dateTime1 = DateTimeOffset.FromUnixTimeSeconds(timestamp1);
                    dateTime2 = DateTimeOffset.FromUnixTimeSeconds(timestamp2);

                    // 提取日期部分 (忽略时间)
                    date1 = dateTime1.Date;
                    date2 = dateTime2.Date;
                    if (date1 != date2)
                        break;
                    lastDaydia += (history[i].Role + ":"+ history[i].Content + ";");
                }
                var body = new ChatRequestClass.ChatReuestBody();
                body.model = ConfigManager.Instance.Network.Model;
                body.messages = new();
                body.messages.Add(new ChatRequestClass.ChatReuestBody.Message() { role = "system", content = $"这是旧的关键记忆:{importantMemories}，这是前一天的对话:{history}，麻烦生成新的关键记忆" });
                body.safe_mode = false;

                body.functions = new();
                body.functions.Add(new GenerateMemory());
                body.function_call = "auto";

                var sendMsgRequest = new ChatRequest();
                sendMsgRequest.Config.URL += "/chat/completions";
                sendMsgRequest.Config.Headers["Authorization"] += $"Bearer {ConfigManager.Instance.Network.apiKey}";
                sendMsgRequest.RequestBody = body;

                NetworkManager.Instance.SendMessage(sendMsgRequest);
            }
        }
        private void OnNewMemoryResponse(string msg)
        {
                var chatResponse = JsonUtility.FromJson<ChatResposeClass.ChatResponse>(msg);
                if (chatResponse != null && chatResponse.choices.Length > 0)
                {
                    foreach (var choice in chatResponse.choices)
                    {
                        if (choice.finish_reason == "function_call")
                        {
                            if (choice.message.function_call.name == "generateMemory")
                            {
                                var arr = JsonUtility.FromJson<GenerateMemory_response_arr>(choice.message.function_call.arguments);
                                if (arr.memory != string.Empty)
                                {
                                    importantMemories = arr.memory;
                                }
                            }
                        }
                    }
                }
        }
        // 添加新的记忆
        public void AddMemory(string content, float importance = 1.0f)
        {
            Memory newMemory = new Memory
            {
                content = content,
                timestamp = DateTime.Now,
                importance = importance
            };

            shortTermMemories.Add(newMemory);

            // 如果超过最大记忆数量，删除最旧的记忆
            if (shortTermMemories.Count > MAX_MEMORIES)
            {
                shortTermMemories.RemoveAt(0);
            }
        }

        // 获取所有记忆
        public List<Memory> GetMemories()
        {
            return new List<Memory>(shortTermMemories);
        }

        // 更新用户评价分数
        public void UpdateUserTrust(float delta)
        {
            userTrustScore = Mathf.Clamp(userTrustScore + delta, 0f, 100f);
        }

        public void UpdateUserAffection(float delta)
        {
            userAffectionScore = Mathf.Clamp(userAffectionScore + delta, 0f, 100f);
        }

        public void UpdateUserRespect(float delta)
        {
            userRespectScore = Mathf.Clamp(userRespectScore + delta, 0f, 100f);
        }

        // 获取总体评价
        public float GetOverallScore()
        {
            return (userTrustScore + userAffectionScore + userRespectScore) / 3f;
        }

        // 根据记忆和评分生成性格特征
        public string GetPersonality()
        {
            float overallScore = GetOverallScore();
            if (overallScore >= 80)
                return "非常友好和信任";
            else if (overallScore >= 60)
                return "友好但有些谨慎";
            else if (overallScore >= 40)
                return "略显疏离";
            else
                return "较为冷淡";
        }
    }
}