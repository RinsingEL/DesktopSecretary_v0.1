using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Framework.Utility;
using Core.Framework.Config;
using Core.Framework.Network;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Com.Module.Chat;
using Core.Framework.FGUI;
using Core.Framework.Network.ChatSystem;
using Newtonsoft.Json;
using Core.Framework.Pet;
using Core.Framework.Network.ChatSystem.Core.Framework.Network.ChatSystem;
using Core.Framework.Plugin;

namespace Com.Module.Watcher
{
    public class WatcherPlugin : PluginBase
    {
        private static WatcherPlugin instance;
        public static WatcherPlugin Instance
        {
            get
            {
                if (instance == null)
                    instance = new WatcherPlugin();
                return instance;
            }
        }

        private string checkCoroutineId; // 检查协程ID
        private float minCheckInterval = 480f; // 最小检查间隔（秒）
        private float maxCheckInterval = 720f; // 最大检查间隔（秒）
        private string currentTaskTitle; // 当前任务标题
        private bool IsFocus = false;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

        private void StartRandomCheck()
        {
            checkCoroutineId = CoroutineManager.Instance.StartManagedCoroutine(RandomCheckCoroutine());
        }

        private IEnumerator RandomCheckCoroutine()
        {
            while (true)
            {
                // 随机等待一段时间
                float waitTime = UnityEngine.Random.Range(minCheckInterval, maxCheckInterval);
                yield return new WaitForSeconds(waitTime);

                // 获取当前活动窗口标题
                string windowTitle = GetActiveWindowTitle();

                // 发送到 GPT 进行专注度分析
                SendToGPT(windowTitle);
            }
        }

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            System.Text.StringBuilder buff = new System.Text.StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();
            if (GetWindowText(handle, buff, nChars) > 0)
            {
                return buff.ToString();
            }
            return "用户没有在进行任何进程";
        }

        public void SendToGPT(string windowTitle)
        {
            if (!IsFocus)
                return;
            var body = new ChatRequestClass.ChatRequestBody();
            body.model = ConfigManager.Instance.Network.Model;
            body.messages = new List<ChatRequestClass.ChatRequestBody.Message>
            {
                new ChatRequestClass.ChatRequestBody.Message
                {
                    role = "system",
                    content = $"{ConfigManager.Instance.Game.Prompt}，当前用户正在进行任务：{currentTaskTitle}。当前好感度为:{Pet.Instance.attributes.GetFavorability()}(最大200，最低0)。请分析用户是否专注于当前任务。当前活动窗口标题是：{windowTitle}。请判断用户是否在专注于当前任务，如果不是，请给出提醒。"
                }
            };
            body.tools = new List<ChatRequestClass.ChatRequestBody.Tool>
            {
                new ChatRequestClass.ChatRequestBody.Tool { type = "function", function = new ChatRequestClass.CheckFocusFunctionCalling() }
            };

            var sendMsgRequest = new ChatRequest();
            sendMsgRequest.Config.URL += "/chat/completions";
            sendMsgRequest.Config.Headers["Authorization"] += $"Bearer {ConfigManager.Instance.Network.apiKey}";
            sendMsgRequest.RequestBody = body;

            string jsonRequest = JsonConvert.SerializeObject(body, Formatting.Indented);
            Debug.Log("Sending watcher request: " + jsonRequest);

            NetworkManager.Instance.SendMessage(sendMsgRequest, OnWatcherResponse);
        }

        private void OnWatcherResponse(bool success)
        {
            if (success)
            {
                NetworkManager.Instance.AddEvent<string>(NetworkEvent.ON_CHAT_RESPONSE, ProcessWatcherResponse);
            }
            else
            {
                Debug.LogError("Watcher request failed.");
            }
        }

        private void ProcessWatcherResponse(string msg)
        {
            var chatResponse = JsonUtility.FromJson<ChatResponseClass.ChatResponse>(msg);
            if (chatResponse != null && chatResponse.choices.Length > 0)
            {
                foreach (var choice in chatResponse.choices)
                {
                    if (choice.finish_reason == "tool_calls" && choice.message.tool_calls != null && choice.message.tool_calls.Length > 0)
                    {
                        var toolCall = choice.message.tool_calls[0];
                        if (toolCall.function.name == "checkFocus")
                        {
                            var focusArgu = JsonUtility.FromJson<ChatResponseClass.ChatResponse.CheckFocusArgu>(toolCall.function.arguments);
                            string replyContent = focusArgu.replyContent;
                            bool focusResult = focusArgu.focusResult;

                            Debug.Log($"Focus Check Result: {focusResult}, Reply: {replyContent}");

                            // 如果用户不专注，显示提醒
                            if (focusResult)
                            {
                                Pet.Instance.attributes.IncreaseFavorability(1f); // 专注加 1
                                ShowReminder(replyContent);
                            }
                            else
                            {
                                Pet.Instance.attributes.DecreaseFavorability(5f); // 不专注减 5
                                ShowReminder(replyContent);
                            }
                        }
                    }
                }
            }
            NetworkManager.Instance.RemoveEvent<string>(NetworkEvent.ON_CHAT_RESPONSE, ProcessWatcherResponse);
        }

        public void ShowReminder(string message)
        {
            var param = new DialoguePanel.DialogueParam
            {
                dialogue = message,
                hide = () => GUIManager.Instance.HideWindow<DialoguePanel>()
            };

            GUIManager.Instance.ShowWindow(param);
        }

        public void SetCurrentTask(string taskTitle)
        {
            currentTaskTitle = taskTitle;
        }
        protected override void OnRegister()
        {
            instance = this;
            StartRandomCheck();
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnUninstall()
        {
            if (!string.IsNullOrEmpty(checkCoroutineId))
            {
                CoroutineManager.Instance.StopManagedCoroutine(checkCoroutineId);
            }
        }

        public void EnterFocus()
        {
            IsFocus = true;
        }
        public void ExitFocus()
        {
            IsFocus = false;
        }
    }
}