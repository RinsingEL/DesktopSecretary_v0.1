using Animation;
using Com.Module.Chat;
using Core.Framework.Config;
using Core.Framework.Event;
using Core.Framework.FGUI;
using Core.Framework.Network;
using Core.Framework.Network.ChatSystem;
using Core.Framework.Plugin;
using Core.Framework.Resource;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

namespace Module.chat
{
    public class ChatPlugin : PluginBase
    {
        private ChatData chatData = new ChatData();

        private static ChatPlugin instance;
        public static ChatPlugin Instance
        {
            get
            {
                if (instance == null)
                    instance = new ChatPlugin();
                return instance;
            }
        }
        protected override void OnRegister()
        {
            instance = this;
            chatData = ChatData.LoadFromLocal();

            EventManager.Instance.AddEvent<string, string>(ClientEvent.ON_SEND_CHAT_REQUEST, OnSendChatMessage);
            EventManager.Instance.AddEvent<string, string, string>(ClientEvent.ON_SEND_FUNC_REQUEST, OnSendFunctionRequest);
            NetworkManager.Instance.AddEvent<string>(NetworkEvent.ON_GPT_RESPONSE,OnGptResponse);
        }
        private void OnGptResponse(string msg)
        {
            var chatResponse = JsonUtility.FromJson<ChatResposeClass.ChatResponse>(msg);
            if (chatResponse != null && chatResponse.choices.Length > 0)
            {
                foreach (var choice in chatResponse.choices)
                {
                    if (choice.finish_reason == "function_call")
                    {
                        if (choice.message.function_call.name == "generateSelectQuery")
                        {
                            var arr = JsonUtility.FromJson<ChatResposeClass.ChatResponse.SelectFunctionArgu>(choice.message.function_call.arguments);
                            OnGPTSelectResponse(arr);
                            if(arr.reply != string.Empty)
                            {
                                DialoguePanel.DialogueParam param = new DialoguePanel.DialogueParam();
                                param.dialogue = arr.reply;
                                GUIManager.Instance.ShowWindow(param);
                                // 添加到聊天记录并保存
                                chatData.Add(choice.message.role, arr.reply);
                                chatData.SaveToLocal();
                            }
                        }
                        else if (choice.message.function_call.name == "generateCrudQuery")
                        {
                            var arr = JsonUtility.FromJson<ChatResposeClass.ChatResponse.CrudFunctionArgu>(choice.message.function_call.arguments);
                            OnSelectSQLGen(arr.generatedQuery, arr.undoQuery);
                            if (arr.reply != string.Empty)
                            {
                                DialoguePanel.DialogueParam param = new DialoguePanel.DialogueParam();
                                param.dialogue = arr.reply;
                                GUIManager.Instance.ShowWindow(param);
                                // 添加到聊天记录并保存
                                chatData.Add(choice.message.role, arr.reply);
                                chatData.SaveToLocal();
                            }
                        }
                        else if(choice.message.function_call.name == "generateReplyWithEmotion")
                        {
                            var arr = JsonUtility.FromJson<ChatResposeClass.ChatResponse.EmotionArgu>(choice.message.function_call.arguments);
                            OnEmotionChange(arr.emotion);
                            DialoguePanel.DialogueParam param = new DialoguePanel.DialogueParam();
                            param.dialogue = arr.replyContent;
                            GUIManager.Instance.ShowWindow(param);
                            // 添加到聊天记录并保存
                            chatData.Add(choice.message.role, arr.replyContent);
                            chatData.SaveToLocal();
                        }
                    }
                    else if (choice.finish_reason == "stop")//防止他不自动调用漏消息
                    {
                        // 添加到聊天记录并保存
                        chatData.Add(choice.message.role, choice.message.content);
                        DialoguePanel.DialogueParam param = new DialoguePanel.DialogueParam();
                        param.dialogue = choice.message.content;
                        GUIManager.Instance.ShowWindow(param);
                        chatData.SaveToLocal();
                    }
                }
            }
        }

        private void OnEmotionChange(string emo)
        {
            EventManager.Instance.Trigger(ClientEvent.ON_PET_EMOTION_CHANGE, emo);
        }

        private void OnSelectSQLGen(string sql, string undo)
        {
            // 检查SQL语句中是否包含不完整的时间格式
            if (Regex.IsMatch(sql, @"\d{4}/\d{2}/\d{2}(?![\s\S]*\d{2}:\d{2}:\d{2})"))
            {
                // 如果发现不完整的时间格式，添加默认时间
                sql = Regex.Replace(sql, @"(\d{4}/\d{2}/\d{2})", "$1 00:00:00");
                Debug.Log("检测到不完整时间格式，已自动补全：" + sql);
            }
            // 如果是 INSERT 语句且不包含 TaskID，就自动生成 UUID
            if (sql.Trim().StartsWith("INSERT INTO Tasks", StringComparison.OrdinalIgnoreCase) &&
                !sql.Contains("TaskID", StringComparison.OrdinalIgnoreCase))
            {
                string uuid = Guid.NewGuid().ToString();
                sql = sql.Replace("INSERT INTO Tasks (", "INSERT INTO Tasks (TaskID, ");
                sql = sql.Replace(") VALUES (", $") VALUES ('{uuid}', ");
            }
            ResourcesManager.Instance.DBSourceManager.ExecuteSql(sql, undo, (bool symbol) => { EventManager.Instance.Trigger(ClientEvent.UPDATE_CALENDAR_INFO); });
        }
        private bool isProcessingQuery = false;

        private void OnGPTSelectResponse(ChatResposeClass.ChatResponse.SelectFunctionArgu arr)
        {
            if (isProcessingQuery) return;
            isProcessingQuery = true;

            // 1. 检查 SQL 是否合法
            if (string.IsNullOrEmpty(arr.generatedSelect))
            {
                OnSendFunctionRequest("生成的SQL语句为空。", arr.intent);
                isProcessingQuery = false;
                return;
            }

            if (!arr.generatedSelect.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                OnSendFunctionRequest("生成的SQL不是查询语句。", arr.intent);
                isProcessingQuery = false;
                return;
            }

            // 2. 处理时间格式
            string sql = arr.generatedSelect;
            if (Regex.IsMatch(sql, @"\d{4}/\d{2}/\d{2}(?![\s\S]*\d{2}:\d{2}:\d{2})"))
            {
                sql = Regex.Replace(sql, @"(\d{4}/\d{2}/\d{2})", "$1 00:00:00");
            }

            // 3. 执行查询
            ResourcesManager.Instance.DBSourceManager.ExecuteSqlQuery(sql, (results) =>
            {
                isProcessingQuery = false;
                if (results != null && results.Count > 0)
                {
                    string resultJson = JsonConvert.SerializeObject(results);
                    OnSendChatMessage($"查询结果：{resultJson}", arr.intent);
                }
                else
                {
                    OnSendChatMessage($"没有找到符合条件的数据。", arr.intent);
                }
            });
        }
        protected override void OnUninstall()
        {
            EventManager.Instance.RemoveEvent<string, string>(ClientEvent.ON_SEND_CHAT_REQUEST, OnSendChatMessage);
            EventManager.Instance.RemoveEvent<string, string, string>(ClientEvent.ON_SEND_FUNC_REQUEST, OnSendFunctionRequest);
            NetworkManager.Instance.RemoveEvent<string>(NetworkEvent.ON_GPT_RESPONSE, OnGptResponse);
        }

        protected override void OnUpdate()
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if(Input.GetKeyDown(KeyCode.G))
                    ShowChatPanel();
            }
        }
        private void ShowChatPanel()
        {
            GUIManager.Instance.ShowWindow<ChatWindow>();
        }

        public void OnSendChatMessage(string prompt, string msg)
        {
            var body = new ChatRequestClass.ChatReuestBody();
            body.model = ConfigManager.Instance.Network.Model;
            body.messages = new();
            body.messages.Add(new ChatRequestClass.ChatReuestBody.Message() { role = "system", content = prompt + "现在的时间是" + DateTime.Now.ToString()});
            body.messages.Add(new ChatRequestClass.ChatReuestBody.Message() { role = "user", content = msg });
            body.safe_mode = false;

            var sendMsgRequest = new ChatRequest();
            sendMsgRequest.Config.URL += "/v1/chat/completions";
            sendMsgRequest.Config.Headers["Authorization"] += $"Bearer {ConfigManager.Instance.Network.apiKey}";
            sendMsgRequest.RequestBody = body;

            NetworkManager.Instance.SendMessage(sendMsgRequest);
        }
        public void OnSendFunctionRequest(string prompt, string msg = null, string func = null)
        {
            var body = new ChatRequestClass.ChatReuestBody();
            body.model = ConfigManager.Instance.Network.Model;
            body.messages = new();
            body.messages.Add(new ChatRequestClass.ChatReuestBody.Message() { role = "system", content = prompt + "现在的时间是" + DateTime.Now.ToString() });
            if (msg != null)
            {
                body.messages.Add(new ChatRequestClass.ChatReuestBody.Message() { role = "user", content = msg });
                chatData.Add("user", msg); // 添加用户消息并保存
            }
            body.functions = new()
            {
                new ChatRequestClass.SelectFunctionCalling(),
                new ChatRequestClass.CrudFunctionCalling(),
                new ChatRequestClass.ReplyWithEmotionFunctionCalling()
            };

            if (func != null)
            {
                body.function_call = new ChatRequestClass.ChatReuestBody.FunctionCall()
                {
                    name = func
                };
            }
            else
                body.function_call = "auto";

            var sendMsgRequest = new ChatRequest();
            sendMsgRequest.Config.URL += "/v1/chat/completions";
            sendMsgRequest.Config.Headers["Authorization"] += $"Bearer {ConfigManager.Instance.Network.apiKey}";
            sendMsgRequest.RequestBody = body;

            NetworkManager.Instance.SendMessage(sendMsgRequest, (bool value) =>
            {
                if (!value)
                {
                    // 如果发送失败，移除最后一条用户消息
                    if (msg != null && chatData.History.Count > 0)
                    {
                        chatData.History.RemoveAt(chatData.History.Count - 1);
                        chatData.SaveToLocal();
                    }
                }
            });
        }
        public List<ChatData.ChatMessage> GetChatHistory()
        {
            return chatData.History;
        }
    }
}