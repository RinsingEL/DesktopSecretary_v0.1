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

        // �¼��ֵ䣬���ڴ洢�¼�����󶨵ķ���
        private Dictionary<string, Delegate> eventDictionary = new Dictionary<string, Delegate>();

        #region �¼�������

        // AddEvent - �޲���
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

        // AddEvent - 1������
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

        // AddEvent - 2������
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

        // RemoveEvent - �޲���
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

        // RemoveEvent - 1������
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

        // RemoveEvent - 2������
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

        // TriggerEvent - �޲���
        private void TriggerEvent(string eventName)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                (del as Action)?.Invoke();
            }
        }

        // TriggerEvent - 1������
        private void TriggerEvent<T>(string eventName, T param)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                (del as Action<T>)?.Invoke(param);
            }
        }

        // TriggerEvent - 2������
        private void TriggerEvent<T1, T2>(string eventName, T1 param1, T2 param2)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                (del as Action<T1, T2>)?.Invoke(param1, param2);
            }
        }

        #endregion

        // ��������
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
                callback?.Invoke(responseData); // ִ�лص�
                TriggerEvent(eventName, responseData); // �����¼�
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