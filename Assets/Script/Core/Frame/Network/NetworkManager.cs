using Core.Framework.Event;
using Core.Framework.Network.ChatSystem;
using Core.Framework.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
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
        private Dictionary<string, Delegate> eventDictionary = new Dictionary<string, Delegate>();

        #region 事件管理方法

        // AddEvent - 无参数
        public void AddEvent(string eventName, Action handler)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                eventDictionary[eventName] = (Action)del + handler;
            }
            else
            {
                eventDictionary[eventName] = handler;
            }
        }

        // AddEvent - 1个参数
        public void AddEvent<T>(string eventName, Action<T> handler)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                eventDictionary[eventName] = (Action<T>)del + handler;
            }
            else
            {
                eventDictionary[eventName] = handler;
            }
        }

        // AddEvent - 2个参数
        public void AddEvent<T1, T2>(string eventName, Action<T1, T2> handler)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                eventDictionary[eventName] = (Action<T1, T2>)del + handler;
            }
            else
            {
                eventDictionary[eventName] = handler;
            }
        }

        // RemoveEvent - 无参数
        public void RemoveEvent(string eventName, Action handler)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                eventDictionary[eventName] = (Action)del - handler;
                if (eventDictionary[eventName] == null)
                {
                    eventDictionary.Remove(eventName);
                }
            }
        }

        // RemoveEvent - 1个参数
        public void RemoveEvent<T>(string eventName, Action<T> handler)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                eventDictionary[eventName] = (Action<T>)del - handler;
                if (eventDictionary[eventName] == null)
                {
                    eventDictionary.Remove(eventName);
                }
            }
        }

        // RemoveEvent - 2个参数
        public void RemoveEvent<T1, T2>(string eventName, Action<T1, T2> handler)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                eventDictionary[eventName] = (Action<T1, T2>)del - handler;
                if (eventDictionary[eventName] == null)
                {
                    eventDictionary.Remove(eventName);
                }
            }
        }

        // TriggerEvent - 无参数
        private void TriggerEvent(string eventName)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                (del as Action)?.Invoke();
            }
        }

        // TriggerEvent - 1个参数
        private void TriggerEvent<T>(string eventName, T param)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                (del as Action<T>)?.Invoke(param);
            }
        }

        // TriggerEvent - 2个参数
        private void TriggerEvent<T1, T2>(string eventName, T1 param1, T2 param2)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                (del as Action<T1, T2>)?.Invoke(param1, param2);
            }
        }

        #endregion

        // 发送请求
        public void SendMessage<T>(RequestBase request, string eventName, Action<T> callback = null) where T : class
        {
            CoroutineManager.Instance.StartManagedCoroutine(SendRequestCoroutine(request, eventName, callback));
        }

        public void SendMessage(RequestBase request, Action<bool> callback = null)
        {
            CoroutineManager.Instance.StartManagedCoroutine(SendRequestCoroutine(request , callback));
        }

        private IEnumerator SendRequestCoroutine<T>(RequestBase request, string eventName, Action<T> callback) where T : class
        {
            UnityWebRequest webRequest = request.CreateWebRequest();
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string responseText = webRequest.downloadHandler.text;
                T responseData = JsonUtility.FromJson<T>(responseText);
                callback?.Invoke(responseData); // 执行回调
                TriggerEvent(eventName, responseData); // 触发事件
            }
            else
            {
                Debug.LogError($"Request Failed: {webRequest.error}");
            }
        }

        private IEnumerator SendRequestCoroutine(RequestBase request , Action<bool> callback = null)
        {
            UnityWebRequest webRequest = request.CreateWebRequest();
            yield return webRequest.SendWebRequest();

            if (webRequest.responseCode == 200)
            {
                string responseText = webRequest.downloadHandler.text;
                if (webRequest.url == "https://oa.api2d.net/v1/chat/completions")
                {
                    TriggerEvent(NetworkEvent.ON_GPT_RESPONSE, responseText);
                }
            }
            else
            {
                Debug.LogError(webRequest.error);
                Debug.LogError(webRequest.downloadHandler.text);
            }
            callback?.Invoke(webRequest.result == UnityWebRequest.Result.Success);
        }
    }
}