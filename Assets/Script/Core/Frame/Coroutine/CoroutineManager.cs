using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Framework.Utility
{
    public class CoroutineManager : MonoBehaviour
    {
        private static CoroutineManager _instance;
        public static CoroutineManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("CoroutineManager");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<CoroutineManager>();
                }
                return _instance;
            }
        }

        // 协程池（Key为协程ID）
        private Dictionary<string, Coroutine> _coroutines = new Dictionary<string, Coroutine>();

        /// <summary>
        /// 启动协程并管理其生命周期
        /// </summary>
        public string StartManagedCoroutine(IEnumerator routine)
        {
            var coroutineId = System.Guid.NewGuid().ToString();
            var coroutine = StartCoroutine(WrappedCoroutine(coroutineId, routine));
            _coroutines.Add(coroutineId, coroutine);
            return coroutineId;
        }

        /// <summary>
        /// 停止指定协程
        /// </summary>
        public void StopManagedCoroutine(string coroutineId)
        {
            if (_coroutines.TryGetValue(coroutineId, out var coroutine))
            {
                StopCoroutine(coroutine);
                _coroutines.Remove(coroutineId);
            }
        }

        /// <summary>
        /// 延迟执行
        /// </summary>
        public string Delay(float seconds, System.Action callback)
        {
            return StartManagedCoroutine(DelayRoutine(seconds, callback));
        }

        /// <summary>
        /// 间隔执行
        /// </summary>
        public string Interval(float interval, System.Action callback, int repeatCount = -1)
        {
            return StartManagedCoroutine(IntervalRoutine(interval, callback, repeatCount));
        }

        // 协程包装器，用完删掉
        private IEnumerator WrappedCoroutine(string coroutineId, IEnumerator routine)
        {
            yield return routine;
            _coroutines.Remove(coroutineId);
        }

        // 延迟执行协程
        private IEnumerator DelayRoutine(float seconds, System.Action callback)
        {
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }

        // 间隔执行协程
        private IEnumerator IntervalRoutine(float interval, System.Action callback, int repeatCount)
        {
            int count = 0;
            while (repeatCount < 0 || count < repeatCount)
            {
                yield return new WaitForSeconds(interval);
                callback?.Invoke();
                count++;
            }
        }
    }
}