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

namespace Com.Module.Watcher
{
    public class WatcherPlugin : MonoBehaviour
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
        private float minCheckInterval = 60f; // 最小检查间隔（秒）
        private float maxCheckInterval = 360f; // 最大检查间隔（秒）
        private string currentTaskTitle; // 当前任务标题

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

        private void Start()
        {
            instance = this;
            StartRandomCheck();
        }

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
                
                // 发送到GPT进行分析
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
            return "";
        }

        public void SendToGPT(string windowTitle)
        {
            var request = new WatcherRequest();
            var body = new WatcherRequest.WatcherRequestBody
            {
                messages = new WatcherRequest.Message[]
                {
                    new WatcherRequest.Message
                    {
                        role = "system",
                        content = $"你是一个专注度监督助手。当前用户正在进行任务：{currentTaskTitle}。请分析用户是否专注于当前任务。"
                    },
                    new WatcherRequest.Message
                    {
                        role = "user",
                        content = $"当前活动窗口标题是：{windowTitle}。请判断用户是否在专注于当前任务，如果不是，请给出提醒。"
                    }
                }
            };

            request.Config.Headers["Authorization"] += ConfigManager.Instance.Network.apiKey;
            request.RequestBody = body;

            NetworkManager.Instance.SendMessage<WatcherResponse>(
                request, 
                "ON_WATCHER_RESPONSE", 
                OnWatcherResponse
            );
        }

        private void OnWatcherResponse(WatcherResponse response)
        {
            if (response != null && response.choices.Length > 0)
            {
                string content = response.choices[0].message.content;
                // 如果GPT认为用户没有专注于任务，显示提醒
                if (content.Contains("不专注") || content.Contains("分心") || content.Contains("提醒"))
                {
                    ShowReminder(content);
                }
            }
        }

        public void ShowReminder(string message)
        {
            // 创建对话窗口参数
            var param = new DialoguePanel.DialogueParam
            {
                dialogue = message,
                hide = () => GUIManager.Instance.HideWindow<DialoguePanel>()
            };

            // 显示对话窗口
            GUIManager.Instance.ShowWindow(param);
        }

        public void SetCurrentTask(string taskTitle)
        {
            currentTaskTitle = taskTitle;
        }

        private void OnDestroy()
        {
            if (!string.IsNullOrEmpty(checkCoroutineId))
            {
                CoroutineManager.Instance.StopManagedCoroutine(checkCoroutineId);
            }
        }
    }

    [Serializable]
    public class WatcherResponse
    {
        public Choice[] choices;
    }

    [Serializable]
    public class Choice
    {
        public Message message;
    }

    [Serializable]
    public class Message
    {
        public string content;
    }
}
