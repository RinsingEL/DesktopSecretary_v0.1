using System;
using System.Collections.Generic;
namespace Core.Framework.Event
{
    public class EventManager
    {
        private static EventManager _instance;
        public static EventManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EventManager();
                }
                return _instance;
            }
        }

        // �洢�����¼����ֵ�
        private Dictionary<string, Delegate> eventDictionary;

        private EventManager()
        {
            eventDictionary = new Dictionary<string, Delegate>();
        }

        // Trigger���� - �޲���
        public void Trigger(string eventName)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                (del as Action)?.Invoke();
            }
        }

        // Trigger���� - 1������
        public void Trigger<T>(string eventName, T param1)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                (del as Action<T>)?.Invoke(param1);
            }
        }

        // Trigger���� - 2������
        public void Trigger<T1, T2>(string eventName, T1 param1, T2 param2)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                (del as Action<T1, T2>)?.Invoke(param1, param2);
            }
        }

        // AddEvent���� - �޲���
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

        // AddEvent���� - 1������
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

        // AddEvent���� - 2������
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

        // RemoveEvent���� - �޲���
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

        // RemoveEvent���� - 1������
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

        // RemoveEvent���� - 2������
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

        // ��������¼�
        public void Clear()
        {
            eventDictionary.Clear();
        }
        //����3���ģ�Ԫ�鲻���ð�
        internal void AddEvent<T1, T2, T3>(string eventName, Action<T1, T2 , T3> handler)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                eventDictionary[eventName] = (Action<T1, T2 , T3>)del + handler;
            }
            else
            {
                eventDictionary[eventName] = handler;
            }
        }

        internal void RemoveEvent<T1, T2, T3>(string eventName, Action<T1, T2, T3> handler)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                eventDictionary[eventName] = (Action<T1, T2 , T3>)del - handler;
                if (eventDictionary[eventName] == null)
                {
                    eventDictionary.Remove(eventName);
                }
            }
        }

        internal void Trigger<T1, T2, T3>(string eventName, T1 param1, T2 param2 , T3 param3)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                (del as Action<T1, T2 , T3>)?.Invoke(param1, param2 , param3);
            }
        }
    }

    // ���ڶ����¼����ľ�̬��linww�������ĳ�int�ģ���string̫����
    public static class ClientEvent
    {
        // ���е�ȫ���¼�
        public const string ON_MONTH_CHANGED = "ON_MONTH_CHANGED";

        // View �������¼�
        public const string ON_DAY_CLICKED = "ON_DAY_CLICKED"; // �������

        // Presenter �������¼�
        public const string UPDATE_DETAIL_VIEW = "UPDATE_DETAIL_VIEW"; // ����������ͼ
        public const string REFRESH_CALENDAR = "REFRESH_CALENDAR";     // ˢ������
        public const string ON_SEND_CHAT_REQUEST = "ON_SEND_CHAT_REQUEST";//������Ϣ��
        public const string ON_SEND_FUNC_REQUEST = "ON_SEND_FUNC_REQUEST";//������Ϣ��
        public const string UPDATE_CALENDAR_REQUEST = "ON_CALENDAR_INFO_REQUEST";
        public const string UPDATE_CALENDAR_VIEW = "UPDATE_CALENDAR_VIEW";
        public const string UPDATE_CALENDAR_INFO = "UPDATE_CALENDAR_INFO";//���´洢������

        public const string ON_GLOBAL_CFG_CHANGE = "ON_GLOBAL_CFG_CHANGE";

        //����
        public const string ON_CLICK_PET = "ON_CLICK_PET";
        public const string ON_PET_EMOTION_CHANGE = "ON_PET_EMOTION_CHANGE";
    }
}