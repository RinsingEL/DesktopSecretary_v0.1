using System;
using System.Collections.Generic;
namespace Core.Framework.Event
{
    // 事件管理器单例类
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

        // 存储所有事件的字典
        private Dictionary<string, Delegate> eventDictionary;

        private EventManager()
        {
            eventDictionary = new Dictionary<string, Delegate>();
        }

        // Trigger方法 - 无参数
        public void Trigger(string eventName)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                (del as Action)?.Invoke();
            }
        }

        // Trigger方法 - 1个参数
        public void Trigger<T>(string eventName, T param1)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                (del as Action<T>)?.Invoke(param1);
            }
        }

        // Trigger方法 - 2个参数
        public void Trigger<T1, T2>(string eventName, T1 param1, T2 param2)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate del))
            {
                (del as Action<T1, T2>)?.Invoke(param1, param2);
            }
        }

        // AddEvent方法 - 无参数
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

        // AddEvent方法 - 1个参数
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

        // AddEvent方法 - 2个参数
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

        // RemoveEvent方法 - 无参数
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

        // RemoveEvent方法 - 1个参数
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

        // RemoveEvent方法 - 2个参数
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

        // 清空所有事件
        public void Clear()
        {
            eventDictionary.Clear();
        }
    }

    // 用于定义事件名的静态类
    public static class ClientEvent
    {
        // 已有的全局事件
        public const string ON_MONTH_CHANGED = "ON_MONTH_CHANGED";

        // View 触发的事件
        public const string ON_DAY_CLICKED = "ON_DAY_CLICKED"; // 点击日期

        // Presenter 触发的事件
        public const string UPDATE_DETAIL_VIEW = "UPDATE_DETAIL_VIEW"; // 更新详情视图
        public const string REFRESH_CALENDAR = "REFRESH_CALENDAR";     // 刷新日历
        public const string ON_SEND_CHAT_REQUEST = "ON_SEND_CHAT_REQUEST";//发送信息后
        public const string UPDATE_CALENDAR_REQUEST = "ON_CALENDAR_INFO_REQUEST";
        public const string UPDATE_CALENDAR_VIEW = "UPDATE_CALENDAR_VIEW";
        public const string UPDATE_CALENDAR_INFO = "UPDATE_CALENDAR_INFO";//更新存储的数据
        // 在这里添加事件名
    }
}