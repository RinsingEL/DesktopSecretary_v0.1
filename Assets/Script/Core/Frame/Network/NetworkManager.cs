using Core.Framework.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;

namespace Core.Framework.Network
{
    public class NetworkManager : MonoBehaviour
    {
        private static NetworkManager _instance;
        public static NetworkManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("NetworkManager");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<NetworkManager>();
                }
                return _instance;
            }
        }

        // 事件字典，用于存储事件和其绑定的方法
        private Dictionary<NetworkEvent, List<Action<string>>> eventHandlers = new Dictionary<NetworkEvent, List<Action<string>>>();

        /// <summary>
        /// 添加事件回调
        /// </summary>
        /// <param name="networkEvent">网络事件类型</param>
        /// <param name="handler">回调方法</param>
        public void AddEvent(NetworkEvent networkEvent, Action<string> handler)
        {
            if (!eventHandlers.ContainsKey(networkEvent))
            {
                eventHandlers[networkEvent] = new List<Action<string>>();
            }
            eventHandlers[networkEvent].Add(handler);
        }

        /// <summary>
        /// 移除事件回调
        /// </summary>
        /// <param name="networkEvent">网络事件类型</param>
        /// <param name="handler">回调方法</param>
        public void RemoveEvent(NetworkEvent networkEvent, Action<string> handler)
        {
            if (eventHandlers.ContainsKey(networkEvent))
            {
                eventHandlers[networkEvent].Remove(handler);
                if (eventHandlers[networkEvent].Count == 0)
                {
                    eventHandlers.Remove(networkEvent);
                }
            }
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="networkEvent">网络事件类型</param>
        /// <param name="response">响应内容</param>
        private void TriggerEvent(NetworkEvent networkEvent, string response)
        {
            if (eventHandlers.ContainsKey(networkEvent))
            {
                foreach (var handler in eventHandlers[networkEvent])
                {
                    handler.Invoke(response); // 执行所有绑定的回调方法
                }
            }
        }
        // 发送请求
        public void SendMessage(RequestBase request, NetworkEvent networkEvent)
        {
            StartCoroutine(SendRequestCoroutine(request, networkEvent));
        }
        public void SendMessage(RequestBase request)
        {
            StartCoroutine(SendRequestCoroutine(request));
        }
        private IEnumerator SendRequestCoroutine(RequestBase request, NetworkEvent networkEvent)
        {
            UnityWebRequest webRequest = request.CreateWebRequest();
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                TriggerEvent(networkEvent, webRequest.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"Request Failed: {webRequest.error}");
            }
        }
        private IEnumerator SendRequestCoroutine(RequestBase request)
        {
            UnityWebRequest webRequest = request.CreateWebRequest();
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("成功收到信息");
            }
            else
            {
                Debug.LogError($"Request Failed: {webRequest.error}");
            }
        }
    }
}
