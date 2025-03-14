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
using System.Collections.Generic;
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
                        }
                        else if (choice.message.function_call.name == "generateCrudQuery")
                        {
                            var arr = JsonUtility.FromJson<ChatResposeClass.ChatResponse.CrudFunctionArgu>(choice.message.function_call.arguments);
                            OnSelectSQLGen(arr.generatedQuery, arr.undoQuery);
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
            ResourcesManager.Instance.DBSourceManager.ExecuteSql(sql,undo);
        }
        private void OnGPTSelectResponse(ChatResposeClass.ChatResponse.SelectFunctionArgu arr)
        {
            string sql = arr.generatedSelect; // 假设 SelectFunctionArgu 中有 generatedSelect 字段

            // 执行 SQL 查询
            ResourcesManager.Instance.DBSourceManager.ExecuteSqlQuery(sql, (results) =>
            {
                if (results != null && results.Count > 0)
                {
                    // 处理查询结果
                    string resultJson = JsonConvert.SerializeObject(results); // 将结果转为 JSON 字符串
                    Debug.Log("SQL res: " + resultJson);
                    OnSendFunctionRequest($"用户此前的要求是\"{arr.intent}\"，下面是JSON格式的查询结果：{resultJson}");
                }
                else
                {
                    Debug.LogWarning("没有结果.");
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
            body.messages.Add(new ChatRequestClass.ChatReuestBody.Message() { role = "system", content = prompt });
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
            body.messages.Add(new ChatRequestClass.ChatReuestBody.Message() { role = "system", content = prompt });
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