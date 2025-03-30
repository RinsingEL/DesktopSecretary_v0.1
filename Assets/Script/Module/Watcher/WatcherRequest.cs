using Core.Framework.Network;
using System;
using System.Collections.Generic;

namespace Com.Module.Watcher
{
    public class WatcherRequest : RequestBase
    {
        public class WatcherRequestBody
        {
            public string model = "gpt-3.5-turbo";
            public Message[] messages;
            public bool safe_mode = false;
        }

        public class Message
        {
            public string role;
            public string content;
        }

        public WatcherRequest()
        {
            Config = new RequestConfig
            {
                URL = "https://oa.api2d.net/v1/chat/completions",
                Method = HttpMethod.POST,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Authorization", "Bearer " }
                }
            };
        }
    }
} 