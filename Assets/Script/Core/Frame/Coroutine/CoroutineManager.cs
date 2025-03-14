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

        // Э�̳أ�KeyΪЭ��ID��
        private Dictionary<string, Coroutine> _coroutines = new Dictionary<string, Coroutine>();

        /// <summary>
        /// ����Э�̲���������������
        /// </summary>
        public string StartManagedCoroutine(IEnumerator routine)
        {
            var coroutineId = System.Guid.NewGuid().ToString();
            var coroutine = StartCoroutine(WrappedCoroutine(coroutineId, routine));
            _coroutines.Add(coroutineId, coroutine);
            return coroutineId;
        }

        /// <summary>
        /// ָֹͣ��Э��
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
        /// �ӳ�ִ��
        /// </summary>
        public string Delay(float seconds, System.Action callback)
        {
            return StartManagedCoroutine(DelayRoutine(seconds, callback));
        }

        /// <summary>
        /// ���ִ��
        /// </summary>
        public string Interval(float interval, System.Action callback, int repeatCount = -1)
        {
            return StartManagedCoroutine(IntervalRoutine(interval, callback, repeatCount));
        }

        // Э�̰�װ��������ɾ��
        private IEnumerator WrappedCoroutine(string coroutineId, IEnumerator routine)
        {
            yield return routine;
            _coroutines.Remove(coroutineId);
        }

        // �ӳ�ִ��Э��
        private IEnumerator DelayRoutine(float seconds, System.Action callback)
        {
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }

        // ���ִ��Э��
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