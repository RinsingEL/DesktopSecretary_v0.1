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
using UnityEngine;
using System.Collections;
using System.IO;
using Core.Framework.Utility;
using Core.Framework.Pet;

namespace Module.chat
{
    public class ChatPlugin : PluginBase
    {
        private ChatData chatData = new ChatData();
        private static ChatPlugin instance;
        private readonly string audioFilePath = @"E:\PythonProject\AudioOutput\audio.wav";

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
            NetworkManager.Instance.AddEvent<string>(NetworkEvent.ON_CHAT_RESPONSE, OnGptResponse);
        }

        private void OnGptResponse(string msg)
        {
            var chatResponse = JsonUtility.FromJson<ChatResposeClass.ChatResponse>(msg);
            if (chatResponse != null && chatResponse.choices.Length > 0)
            {
                foreach (var choice in chatResponse.choices)
                {
                    string replyText = null;

                    if (choice.finish_reason == "function_call")
                    {
                        if (choice.message.function_call.name == "generateSelectQuery")
                        {
                            var arr = JsonUtility.FromJson<ChatResposeClass.ChatResponse.SelectFunctionArgu>(choice.message.function_call.arguments);
                            OnGPTSelectResponse(arr);
                            if (arr.reply != string.Empty)
                            {
                                replyText = arr.reply;
                            }
                        }
                        else if (choice.message.function_call.name == "generateCrudQuery")
                        {
                            var arr = JsonUtility.FromJson<ChatResposeClass.ChatResponse.CrudFunctionArgu>(choice.message.function_call.arguments);
                            OnSelectSQLGen(arr.generatedQuery, arr.undoQuery);
                            if (arr.reply != string.Empty)
                            {
                                replyText = arr.reply;
                            }
                        }
                        else if (choice.message.function_call.name == "generateReplyWithEmotion")
                        {
                            var arr = JsonUtility.FromJson<ChatResposeClass.ChatResponse.EmotionArgu>(choice.message.function_call.arguments);
                            OnEmotionChange(arr.emotion);
                            replyText = arr.replyContent;
                        }
                    }
                    else if (choice.finish_reason == "stop")
                    {
                        replyText = choice.message.content;
                    }

                    if (!string.IsNullOrEmpty(replyText))
                    {
                        DialoguePanel.DialogueParam param = new DialoguePanel.DialogueParam();
                        param.dialogue = replyText;
                        GUIManager.Instance.ShowWindow(param);

                        chatData.Add(choice.message.role, replyText);
                        chatData.SaveToLocal();

                        // 使用协程合成并播放语音
                        CoroutineManager.Instance.StartManagedCoroutine(SynthesizeAndPlayAudio(replyText));
                    }
                }
            }
        }

        /// <summary>
        /// 协程：合成并播放语音，播放后删除文件
        /// </summary>
        private IEnumerator SynthesizeAndPlayAudio(string text)
        {
            SynthesizerController synthesizer = GameObject.FindObjectOfType<SynthesizerController>();
            if (synthesizer == null)
            {
                Debug.LogError("SynthesizerController 未找到，请确保场景中存在该组件");
                yield break;
            }

            synthesizer.Synthesize(text);
            int overTime = 0;

            while (!File.Exists(audioFilePath) && overTime < 1000)
            {
                overTime++;
                yield return new WaitForSeconds(0.1f);
            }
            overTime = 0;
            // 播放音频
            SpeechManager.Instance.PlayDynamicSound(audioFilePath);

            // 等待播放完成
            while (SpeechManager.Instance.IsPlaying() && overTime < 1000)
            {
                overTime++;
                yield return new WaitForSeconds(0.1f);  // 每 100ms 检查一次
            }

            // 删除音频文件
            if (File.Exists(audioFilePath))
            {
                File.Delete(audioFilePath);
                Debug.Log($"SpeechManager: 已删除音频文件: {audioFilePath}");
            }
        }

        private void OnEmotionChange(string emo)
        {
            EventManager.Instance.Trigger(ClientEvent.ON_PET_EMOTION_CHANGE, emo);
        }

        private void OnSelectSQLGen(string sql, string undo)
        {
            if (Regex.IsMatch(sql, @"\d{4}/\d{2}/\d{2}(?![\s\S]*\d{2}:\d{2}:\d{2})"))
            {
                sql = Regex.Replace(sql, @"(\d{4}/\d{2}/\d{2})", "$1 00:00:00");
                Debug.Log("检测到不完整时间格式，已自动补全：" + sql);
            }
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

            string sql = arr.generatedSelect;
            if (Regex.IsMatch(sql, @"\d{4}/\d{2}/\d{2}(?![\s\S]*\d{2}:\d{2}:\d{2})"))
            {
                sql = Regex.Replace(sql, @"(\d{4}/\d{2}/\d{2})", "$1 00:00:00");
            }

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
            NetworkManager.Instance.RemoveEvent<string>(NetworkEvent.ON_CHAT_RESPONSE, OnGptResponse);
        }

        protected override void OnUpdate()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.G))
                ShowChatPanel();
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
            body.messages.Add(new ChatRequestClass.ChatReuestBody.Message() { role = "system", content = $"这是角色提示词{prompt}，现在的时间是{DateTime.Now.ToString()};以下是长期记忆：{Pet.Instance.attributes.importantMemories}。以下是今天的和用户的历史对话：" + GetChatHistoryLast24HoursAsString() });
            body.messages.Add(new ChatRequestClass.ChatReuestBody.Message() { role = "user", content = msg });
            body.safe_mode = false;

            var sendMsgRequest = new ChatRequest();
            sendMsgRequest.Config.URL += "/chat/completions";
            sendMsgRequest.Config.Headers["Authorization"] += $"Bearer {ConfigManager.Instance.Network.apiKey}";
            sendMsgRequest.RequestBody = body;

            NetworkManager.Instance.SendMessage(sendMsgRequest);
        }

        public void OnSendFunctionRequest(string prompt, string msg = null, string func = null)
        {
            var body = new ChatRequestClass.ChatReuestBody();
            body.model = ConfigManager.Instance.Network.Model;
            body.messages = new();
            body.messages.Add(new ChatRequestClass.ChatReuestBody.Message() { role = "system",
                content = $"这是角色提示词{prompt}，现在的时间是{DateTime.Now.ToString()};以下是长期记忆：{Pet.Instance.attributes.importantMemories}。以下是今天的和用户的历史对话："+ GetChatHistoryLast24HoursAsString()});
            if (msg != null)
            {
                body.messages.Add(new ChatRequestClass.ChatReuestBody.Message() { role = "user", content = msg });
                chatData.Add("user", msg);
            }
            body.functions = new()
            {
                new ChatRequestClass.SelectFunctionCalling(),
                new ChatRequestClass.CrudFunctionCalling(),
                new ChatRequestClass.ReplyWithEmotionFunctionCalling()
            };

            if (func != null)
            {
                body.function_call = new ChatRequestClass.ChatReuestBody.FunctionCall() { name = func };
            }
            else
                body.function_call = "auto";

            var sendMsgRequest = new ChatRequest();
            sendMsgRequest.Config.URL += "/vchat/completions";
            sendMsgRequest.Config.Headers["Authorization"] += $"Bearer {ConfigManager.Instance.Network.apiKey}";
            sendMsgRequest.RequestBody = body;

            NetworkManager.Instance.SendMessage(sendMsgRequest, (bool value) =>
            {
                if (!value && msg != null && chatData.History.Count > 0)
                {
                    chatData.History.RemoveAt(chatData.History.Count - 1);
                    chatData.SaveToLocal();
                }
            });
        }
        public string GetChatHistoryLast24HoursAsString()
        {
            long now = DateTimeOffset.Now.ToUnixTimeSeconds();
            long twentyFourHoursAgo = now - 86400; // 24小时 = 86400秒
            var recentMessages = chatData.History
                .FindAll(msg => msg.Timestamp >= twentyFourHoursAgo)
                .ConvertAll(msg => $"{msg.Role}: {msg.Content}");
            return string.Join("\n", recentMessages);
        }
        public List<ChatData.ChatMessage> GetChatHistory()
        {
            return chatData.History;
        }
    }
}