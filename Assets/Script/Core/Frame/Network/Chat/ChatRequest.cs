using System.Collections.Generic;

namespace Core.Framework.Network.ChatSystem
{
    public class ChatRequest : RequestBase
    {
        public ChatRequest()
        {
            Config = new RequestConfig()
            {
                URL = "https://oa.api2d.net",
                Method = HttpMethod.POST,
                Headers = new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json" },
                    {"Authorization","" }
                }
            };
        }
    }
}

