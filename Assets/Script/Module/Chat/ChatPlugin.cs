using Com.Module.Chat;
using Core.Framework.Config;
using Core.Framework.Event;
using Core.Framework.Network;
using Core.Framework.Network.ChatSystem;
using Core.Framework.Plugin;
using FairyGUI;
using System;
using System.Net;
using UnityEngine;

namespace Module.chat
{
    public class ChatPlugin : PluginBase
    {
        ChatData chatData = new ChatData();
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
            EventManager.Instance.AddEvent<string>(ClientEvent.ON_SEND_CHAT_REQUEST, OnSendChatMessage);
        }

        protected override void OnUninstall()
        {
            EventManager.Instance.RemoveEvent<string>(ClientEvent.ON_SEND_CHAT_REQUEST, OnSendChatMessage);
        }

        public void OnSendChatMessage(string msg)
        {
            var body = new ChatClass.ChatReuestBody();
            body.model = ConfigManager.Instance.Network.Model;
            body.messages = new();
            body.messages.Add(new ChatClass.ChatReuestBody.Message() { role = "user", content = msg });
            body.safe_mode = false;
            var sendMsgRequest = new ChatRequest();
            sendMsgRequest.Config.URL += "/v1/chat/completions";
            sendMsgRequest.Config.Headers["Authorization"] += $"Bearer {ConfigManager.Instance.Network.apiKey}";
            sendMsgRequest.RequestBody = body;
            NetworkManager.Instance.SendMessage(sendMsgRequest);
        }
    }
}

