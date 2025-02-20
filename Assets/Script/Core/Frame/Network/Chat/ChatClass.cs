using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Framework.Network;
using System;
namespace Core.Framework.Network.ChatSystem
{
    public static class ChatClass
    {
        [Serializable]
        public class ChatReuestBody
        {
            [Serializable]
            public class Message
            {
                public string role;
                public string content;
                public Message() { }
            }
            public string model;
            public List<Message> messages;
            public bool safe_mode;
        }
    }
}