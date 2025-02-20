using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Core.Framework.Network
{
    public enum HttpMethod { GET, POST, PUT, DELETE }

    public class RequestConfig
    {
        public string URL { get; set; }
        public HttpMethod Method { get; set; } = HttpMethod.POST;
        /// <summary>
        /// 包含所有的请求头部分
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    }
    public abstract class RequestBase
    {
        public RequestConfig Config { get; protected set; }  // 请求配置
        public object RequestBody { get; set; }              // 请求体（动态赋值）

        // 生成 UnityWebRequest
        public UnityWebRequest CreateWebRequest()
        {
            Debug.Log(JsonUtility.ToJson(RequestBody));
            var request = new UnityWebRequest(Config.URL, Config.Method.ToString())
            {
                uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(RequestBody))),
                downloadHandler = new DownloadHandlerBuffer()
            };

            foreach (var header in Config.Headers)
                request.SetRequestHeader(header.Key, header.Value);

            return request;
        }
    }
}
