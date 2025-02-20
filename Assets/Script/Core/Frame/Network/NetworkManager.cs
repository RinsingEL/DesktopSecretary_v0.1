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

        // �¼��ֵ䣬���ڴ洢�¼�����󶨵ķ���
        private Dictionary<NetworkEvent, List<Action<string>>> eventHandlers = new Dictionary<NetworkEvent, List<Action<string>>>();

        /// <summary>
        /// ����¼��ص�
        /// </summary>
        /// <param name="networkEvent">�����¼�����</param>
        /// <param name="handler">�ص�����</param>
        public void AddEvent(NetworkEvent networkEvent, Action<string> handler)
        {
            if (!eventHandlers.ContainsKey(networkEvent))
            {
                eventHandlers[networkEvent] = new List<Action<string>>();
            }
            eventHandlers[networkEvent].Add(handler);
        }

        /// <summary>
        /// �Ƴ��¼��ص�
        /// </summary>
        /// <param name="networkEvent">�����¼�����</param>
        /// <param name="handler">�ص�����</param>
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
        /// �����¼�
        /// </summary>
        /// <param name="networkEvent">�����¼�����</param>
        /// <param name="response">��Ӧ����</param>
        private void TriggerEvent(NetworkEvent networkEvent, string response)
        {
            if (eventHandlers.ContainsKey(networkEvent))
            {
                foreach (var handler in eventHandlers[networkEvent])
                {
                    handler.Invoke(response); // ִ�����а󶨵Ļص�����
                }
            }
        }
        // ��������
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
                Debug.Log("�ɹ��յ���Ϣ");
            }
            else
            {
                Debug.LogError($"Request Failed: {webRequest.error}");
            }
        }
    }
}
