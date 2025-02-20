// FGUIResourceManager.cs
using System.Collections;
using System.Collections.Generic;
using Core.Framework.Utility;
using FairyGUI;
using UnityEngine;

namespace Core.Framework.FGUI
{
    public class FGUIResourceManager : MonoBehaviour
    {
        private static FGUIResourceManager _instance;
        public static FGUIResourceManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("FGUIResourceManager");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<FGUIResourceManager>();
                }
                return _instance;
            }
        }

        // �Ѽ��صİ�������
        private HashSet<string> _loadedPackages = new HashSet<string>();

        // �첽����������٣����� -> Э��ID��
        private Dictionary<string, string> _loadingTasks = new Dictionary<string, string>();

        /// <summary>
        /// ͬ������FGUI��
        /// </summary>
        public void AddPackage(string packageName)
        {
            if (_loadedPackages.Contains(packageName))
            {
                Debug.LogWarning($"Package [{packageName}] already loaded!");
                return;
            }

            UIPackage.AddPackage(packageName);
            _loadedPackages.Add(packageName);
        }

        /// <summary>
        /// �첽����FGUI����ͨ��Э�̹�������
        /// </summary>
        public void AddPackageAsync(string packageName, System.Action onComplete = null)
        {
            if (_loadedPackages.Contains(packageName))
            {
                onComplete?.Invoke();
                return;
            }

            // �����ظ�����
            if (_loadingTasks.ContainsKey(packageName)) return;

            var coroutineId = CoroutineManager.Instance.StartManagedCoroutine(
                LoadPackageRoutine(packageName, onComplete)
            );
            _loadingTasks.Add(packageName, coroutineId);
        }

        /// <summary>
        /// ж�ذ�
        /// </summary>
        public void RemovePackage(string packageName)
        {
            if (_loadedPackages.Contains(packageName))
            {
                UIPackage.RemovePackage(packageName);
                _loadedPackages.Remove(packageName);
            }
        }

        // �첽����Э��
        private IEnumerator LoadPackageRoutine(string packageName, System.Action onComplete)
        {
            // �첽���أ�ʵ�ʼ����߼��������Ŀ������
            yield return null;
            UIPackage.AddPackage(packageName);
            _loadedPackages.Add(packageName);
            _loadingTasks.Remove(packageName);
            onComplete?.Invoke();
        }
    }
}