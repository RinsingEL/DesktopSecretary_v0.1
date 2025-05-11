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
using Core.Framework.Network.ChatSystem.Core.Framework.Network.ChatSystem;
using static Com.Module.Chat.DialoguePanel;
using System.Linq;

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
            var chatResponse = JsonConvert.DeserializeObject<ChatResponseClass.ChatResponse>(msg);
            if (chatResponse != null && chatResponse.choices.Length > 0)
            {
                foreach (var choice in chatResponse.choices)
                {
                    string replyText = null;

                    if (choice.finish_reason == "tool_calls" && choice.message.tool_calls != null && choice.message.tool_calls.Length > 0)
                    {
                        var toolCall = choice.message.tool_calls[0];

                        if (toolCall.function.name == "generateSelectQuery")
                        {
                            var arguments = JsonConvert.DeserializeObject<ChatResponseClass.ChatResponse.SelectFunctionArgu>(toolCall.function.arguments);
                            OnGPTSelectResponse(arguments);
                            if (!string.IsNullOrEmpty(arguments.reply))
                            {
                                replyText = arguments.reply;
                                //SendToolResponse(toolCall.id, arguments.reply);
                            }
                        }
                        else if (toolCall.function.name == "generateCrudQuery")
                        {
                            var arguments = JsonConvert.DeserializeObject<Dictionary<string, string>>(toolCall.function.arguments);
                            OnSelectSQLGen(arguments["generatedQuery"], arguments["undoQuery"]);
                            if (!string.IsNullOrEmpty(arguments.GetValueOrDefault("reply")))
                            {
                                replyText = arguments["reply"];
                               //SendToolResponse(toolCall.id, arguments["reply"]);
                            }
                        }
                        else if (toolCall.function.name == "generateReplyWithEmotion")
                        {
                            try
                            {
                                var arguments = JsonConvert.DeserializeObject<ChatResponseClass.ChatResponse.ReplyWithEmotionArgu>(toolCall.function.arguments);
                                //Pet.Instance.attributes.UpdateFavorability(arguments.favorabilityImpact);
                                // 转换为键值对列表
                                var emotionContentPairs = arguments.emotionContentPairs
                                    .Select(pair => new KeyValuePair<string, string>(pair.emotion, pair.content))
                                    .ToList();
                                if (arguments.favorabilityImpact > 0)
                                    Pet.Instance.attributes.IncreaseFavorability(arguments.favorabilityImpact);
                                else
                                    Pet.Instance.attributes.DecreaseFavorability(-1 * arguments.favorabilityImpact);

                                string fullReplyText = string.Join("\n", emotionContentPairs.Select(pair => pair.Value));
                                CoroutineManager.Instance.StartManagedCoroutine(SynthesizeAndPlayAudio(fullReplyText, () => {                                 
                                    // 异步处理
                                    CoroutineManager.Instance.StartManagedCoroutine(DisplayEmotionContentPairsAsync(emotionContentPairs));
                                }));

                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Failed to deserialize ReplyWithEmotionArgu: {e.Message}");
                                var errorText = "抱歉，处理回复时出错。";

                                CoroutineManager.Instance.StartManagedCoroutine(SynthesizeAndPlayAudio(errorText , () => {
                                    DialoguePanel.DialogueParam param = new DialogueParam { dialogue = errorText };
                                    GUIManager.Instance.ShowWindow(param);
                                    chatData.Add("assistant", errorText);
                                    chatData.SaveToLocal();
                                }));
                            }
                        }
                        else if (toolCall.function.name == "checkFocus")
                        {
                            var arguments = JsonConvert.DeserializeObject<ChatResponseClass.ChatResponse.CheckFocusArgu>(toolCall.function.arguments);
                            replyText = arguments.replyContent;
                            if(arguments.focusResult)
                            {
                                Pet.Instance.attributes.IncreaseFavorability(1);
                            }
                            else
                            {
                                Pet.Instance.attributes.DecreaseFavorability(5);
                            }
                            Debug.Log($"Focus Result: {arguments.focusResult}");
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

                        CoroutineManager.Instance.StartManagedCoroutine(SynthesizeAndPlayAudio(replyText, () => {
                            GUIManager.Instance.ShowWindow(param);

                            chatData.Add(choice.message.role, replyText);
                            chatData.SaveToLocal();
                        }));
                    }
                }
            }
        }
        // 新增协程：异步显示键值对列表
        private IEnumerator DisplayEmotionContentPairsAsync(List<KeyValuePair<string, string>> pairs)
        {
            foreach (var pair in pairs)
            {
                // 显示对话
                DialoguePanel.DialogueParam param = new DialoguePanel.DialogueParam
                {
                    dialogue = pair.Value, // 句子
                };
                GUIManager.Instance.ShowWindow(param);

                // 触发表情变化
                OnEmotionChange(pair.Key);

                // 保存历史
                chatData.Add("assistant", pair.Value);

                // 等待动画完成（固定延迟，2秒）
                yield return new WaitForSeconds(2f);
            }

            // 保存历史
            chatData.SaveToLocal();
        }
        public void SendToolResponse(string toolCallId, string content)
        {
            var body = new ChatRequestClass.ChatRequestBody();
            body.model = ConfigManager.Instance.Network.Model;
            body.messages = new List<ChatRequestClass.ChatRequestBody.Message>(chatData.History.ConvertAll(h => new ChatRequestClass.ChatRequestBody.Message { role = h.Role, content = h.Content }));
            body.messages.Add(new ChatRequestClass.ChatRequestBody.Message { role = "tool", tool_call_id = toolCallId, content = content });
            body.tools = new List<ChatRequestClass.ChatRequestBody.Tool>
    {
        new ChatRequestClass.ChatRequestBody.Tool { type = "function", function = new ChatRequestClass.SelectFunctionCalling() },
        new ChatRequestClass.ChatRequestBody.Tool { type = "function", function = new ChatRequestClass.CrudFunctionCalling() },
        new ChatRequestClass.ChatRequestBody.Tool { type = "function", function = new ChatRequestClass.ReplyWithEmotionFunctionCalling() }
    };

            var sendMsgRequest = new ChatRequest();
            sendMsgRequest.Config.URL += "/chat/completions";
            sendMsgRequest.Config.Headers["Authorization"] += $"Bearer {ConfigManager.Instance.Network.apiKey}";
            sendMsgRequest.RequestBody = body;

            string jsonRequest = JsonConvert.SerializeObject(body, Formatting.Indented);
            Debug.Log("Sending tool response: " + jsonRequest);

            NetworkManager.Instance.SendMessage(sendMsgRequest);
        }

        private IEnumerator SynthesizeAndPlayAudio(string text , Action OnComplite)
        {
            SynthesizerController synthesizer = GameObject.FindObjectOfType<SynthesizerController>();
            if (synthesizer == null)
            {
                Debug.LogError("SynthesizerController 未找到，请确保场景中存在该组件");
                yield break;
            }

            // 触发语音合成
            synthesizer.Synthesize(text);

            // 等待语音文件生成，最多 30 秒
            float maxWaitTime = 15f;
            float elapsedTime = 0f;

            while (!File.Exists(audioFilePath) && elapsedTime < maxWaitTime)
            {
                elapsedTime += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }

            // 检查是否成功生成文件
            if (!File.Exists(audioFilePath))
            {
                Debug.LogError($"语音文件未能在 {maxWaitTime} 秒内生成: {audioFilePath}");
                OnComplite();//linww依旧生成
                yield break;
            }

            // 播放生成的语音
            SpeechManager.Instance.PlayDynamicSound(audioFilePath);
            OnComplite();

            // 等待播放完成，最多 30 秒
            elapsedTime = 0f;
            while (SpeechManager.Instance.IsPlaying() && elapsedTime < maxWaitTime || elapsedTime < 3f)
            {
                elapsedTime += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }

            // 清理音频文件
            if (File.Exists(audioFilePath))
            {
                try
                {
                    File.Delete(audioFilePath);
                    Debug.Log($"SpeechManager: 已删除音频文件: {audioFilePath}");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"删除音频文件失败: {audioFilePath}, 错误: {e.Message}");
                }
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

        private void OnGPTSelectResponse(ChatResponseClass.ChatResponse.SelectFunctionArgu arr)
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
                    OnSendChatMessage($"提醒用户没有在数据库内找到符合条件的数据。", arr.intent);
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
            var body = new ChatRequestClass.ChatRequestBody();
            body.model = ConfigManager.Instance.Network.Model;
            body.messages = new List<ChatRequestClass.ChatRequestBody.Message>();
            body.messages.Add(new ChatRequestClass.ChatRequestBody.Message
            {
                role = "system",
                content = $"这是角色提示词{prompt}，现在的时间是{DateTime.Now.ToString()};以下是长期记忆：{Pet.Instance.attributes.importantMemories}。当前好感度为:{Pet.Instance.attributes.GetFavorability()}(最大200，最低0)。以下是今天的和用户的历史对话：" + GetChatHistoryLast24HoursAsString()
            });
            body.messages.Add(new ChatRequestClass.ChatRequestBody.Message { role = "user", content = msg });
            body.safe_mode = false;

            //var currentConversation = new List<ChatRequestClass.ChatRequestBody.Message>(body.messages);

            var sendMsgRequest = new ChatRequest();
            sendMsgRequest.Config.URL += "/chat/completions";
            sendMsgRequest.Config.Headers["Authorization"] += $"Bearer {ConfigManager.Instance.Network.apiKey}";
            sendMsgRequest.RequestBody = body;

            string jsonRequest = JsonConvert.SerializeObject(body, Formatting.Indented);
            Debug.Log("Sending request: " + jsonRequest);

            NetworkManager.Instance.SendMessage(sendMsgRequest);
        }

        public void OnSendFunctionRequest(string prompt, string msg = null, string toolName = null)
        {
            var body = new ChatRequestClass.ChatRequestBody();
            body.model = ConfigManager.Instance.Network.Model;
            body.messages = new List<ChatRequestClass.ChatRequestBody.Message>();
            body.messages.Add(new ChatRequestClass.ChatRequestBody.Message
            {
                role = "system",
                content = $"这是角色提示词{prompt}，现在的时间是{DateTime.Now.ToString()};以下是长期记忆：{Pet.Instance.attributes.importantMemories}。当前好感度为:{Pet.Instance.attributes.GetFavorability()}(最大200，最低0)。以下是今天的和用户的历史对话：" + GetChatHistoryLast24HoursAsString()
            });
            if (msg != null)
            {
                body.messages.Add(new ChatRequestClass.ChatRequestBody.Message { role = "user", content = msg });
                chatData.Add("user", msg);
            }
            body.safe_mode = false;

            body.tools = new List<ChatRequestClass.ChatRequestBody.Tool>
    {
        new ChatRequestClass.ChatRequestBody.Tool { type = "function", function = new ChatRequestClass.SelectFunctionCalling() },
        new ChatRequestClass.ChatRequestBody.Tool { type = "function", function = new ChatRequestClass.CrudFunctionCalling() },
        new ChatRequestClass.ChatRequestBody.Tool { type = "function", function = new ChatRequestClass.ReplyWithEmotionFunctionCalling() },
        new ChatRequestClass.ChatRequestBody.Tool { type = "function", function = new ChatRequestClass.CheckFocusFunctionCalling() } // 新增专注检测工具
    };

            //var currentConversation = new List<ChatRequestClass.ChatRequestBody.Message>(body.messages);

            var sendMsgRequest = new ChatRequest();
            sendMsgRequest.Config.URL += "/chat/completions";
            sendMsgRequest.Config.Headers["Authorization"] += $"Bearer {ConfigManager.Instance.Network.apiKey}";
            sendMsgRequest.RequestBody = body;

            string jsonRequest = JsonConvert.SerializeObject(body, Formatting.Indented);
            Debug.Log("Sending request: " + jsonRequest);

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
            long twentyFourHoursAgo = now - 86400;
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