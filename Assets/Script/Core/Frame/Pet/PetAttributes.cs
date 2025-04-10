using Core.Framework.Config;
using Core.Framework.Network;
using Core.Framework.Network.ChatSystem;
using Module.chat;
using System;
using System.Collections.Generic;
using UnityEngine;
using Com.Module.Chat;
using Core.Framework.FGUI;
using Core.Framework.Utility;
using Core.Framework.Network.ChatSystem.Core.Framework.Network.ChatSystem;
using Newtonsoft.Json;

namespace Core.Framework.Pet
{
    [Serializable]
    public class Memory
    {
        public string content;
        public DateTime timestamp;
        public float importance;
    }

    public class PetAttributes : MonoBehaviour
    {
        private List<Memory> shortTermMemories = new List<Memory>();
        public string importantMemories;
        private const int MAX_MEMORIES = 10;

        // 好感度系统
        private float favorability = 50f; // 初始值为 50，范围 0-200

        private List<ChatRequestClass.ChatRequestBody.Message> currentConversation = new List<ChatRequestClass.ChatRequestBody.Message>();

        private void Start()
        {
            NetworkManager.Instance.AddEvent<string>(NetworkEvent.ON_CHAT_RESPONSE, OnNewMemoryResponse);
            RefreshMemory();
        }

        // 增加好感度
        public void IncreaseFavorability(float delta)
        {
            favorability = Mathf.Clamp(favorability + delta, 0f, 200f);
        }

        // 减少好感度
        public void DecreaseFavorability(float delta)
        {
            favorability = Mathf.Clamp(favorability - delta, 0f, 200f);
        }

        // 获取当前好感度
        public float GetFavorability()
        {
            return favorability;
        }

        public void RefreshMemory()
        {
            long timestamp1 = DateTimeOffset.Now.ToUnixTimeSeconds();
            var history = ChatPlugin.Instance.GetChatHistory();
            if (history.Count == 0) return;

            long timestamp2 = history[history.Count - 1].Timestamp;
            DateTimeOffset dateTime1 = DateTimeOffset.FromUnixTimeSeconds(timestamp1);
            DateTimeOffset dateTime2 = DateTimeOffset.FromUnixTimeSeconds(timestamp2);

            bool isSameDay = dateTime1.Date == dateTime2.Date;
            if (!isSameDay)
            {
                string lastDayDialog = "";
                timestamp1 = history[history.Count - 1].Timestamp;
                for (int i = history.Count - 2; i >= 0; i--)
                {
                    timestamp2 = history[i].Timestamp;
                    dateTime1 = DateTimeOffset.FromUnixTimeSeconds(timestamp1);
                    dateTime2 = DateTimeOffset.FromUnixTimeSeconds(timestamp2);
                    if (dateTime1.Date != dateTime2.Date)
                        break;
                    lastDayDialog += (history[i].Role + ":" + history[i].Content + ";");
                }

                var body = new ChatRequestClass.ChatRequestBody();
                body.model = ConfigManager.Instance.Network.Model;
                body.messages = new List<ChatRequestClass.ChatRequestBody.Message>();
                body.messages.Add(new ChatRequestClass.ChatRequestBody.Message
                {
                    role = "system",
                    content = $"这是旧的关键记忆:{importantMemories}，这是前一天的对话:{lastDayDialog}，麻烦生成新的关键记忆"
                });
                body.safe_mode = false;

                body.tools = new List<ChatRequestClass.ChatRequestBody.Tool>
                {
                    new ChatRequestClass.ChatRequestBody.Tool { type = "function", function = new GenerateMemory() }
                };

                currentConversation = new List<ChatRequestClass.ChatRequestBody.Message>(body.messages);

                var sendMsgRequest = new ChatRequest();
                sendMsgRequest.Config.URL += "/chat/completions";
                sendMsgRequest.Config.Headers["Authorization"] += $"Bearer {ConfigManager.Instance.Network.apiKey}";
                sendMsgRequest.RequestBody = body;

                string jsonRequest = JsonConvert.SerializeObject(body, Formatting.Indented);
                Debug.Log("Sending request: " + jsonRequest);

                NetworkManager.Instance.SendMessage(sendMsgRequest);
            }
        }

        private void OnNewMemoryResponse(string msg)
        {
            var chatResponse = JsonUtility.FromJson<ChatResponseClass.ChatResponse>(msg);
            if (chatResponse != null && chatResponse.choices.Length > 0)
            {
                foreach (var choice in chatResponse.choices)
                {
                    if (choice.finish_reason == "tool_calls" && choice.message.tool_calls != null && choice.message.tool_calls.Length > 0)
                    {
                        currentConversation.Add(new ChatRequestClass.ChatRequestBody.Message
                        {
                            role = choice.message.role,
                            content = choice.message.content
                        });

                        var toolCall = choice.message.tool_calls[0];
                        if (toolCall.function.name == "generateMemory")
                        {
                            var arr = JsonUtility.FromJson<GenerateMemoryResponseArgu>(toolCall.function.arguments);
                            if (!string.IsNullOrEmpty(arr.memory))
                            {
                                importantMemories = arr.memory;
                                ChatPlugin.Instance.SendToolResponse(toolCall.id, arr.memory);
                            }
                        }
                    }
                }
            }
        }

        public void AddMemory(string content, float importance = 1.0f)
        {
            Memory newMemory = new Memory
            {
                content = content,
                timestamp = DateTime.Now,
                importance = importance
            };

            shortTermMemories.Add(newMemory);

            if (shortTermMemories.Count > MAX_MEMORIES)
            {
                shortTermMemories.RemoveAt(0);
            }
        }

        public List<Memory> GetMemories()
        {
            return new List<Memory>(shortTermMemories);
        }
    }

    [Serializable]
    public class GenerateMemory
    {
        public string name = "generateMemory";
        public string description = "根据旧的关键记忆和前一天的对话生成新的关键记忆。";
        public Parameters parameters = new Parameters();

        [Serializable]
        public class Parameters
        {
            public string type = "object";
            public Properties properties = new Properties();
            public string[] required = new string[] { "memory" };
            public bool additionalProperties = false;
        }

        [Serializable]
        public class Properties
        {
            public Property memory = new Property { type = "string", description = "新的关键记忆内容" };
        }

        [Serializable]
        public class Property
        {
            public string type;
            public string description;
        }
    }

    [Serializable]
    public class GenerateMemoryResponseArgu
    {
        public string memory;
    }
}