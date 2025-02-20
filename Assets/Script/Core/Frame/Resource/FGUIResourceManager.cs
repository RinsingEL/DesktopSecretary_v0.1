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

        // 已加载的包名缓存
        private HashSet<string> _loadedPackages = new HashSet<string>();

        // 异步加载任务跟踪（包名 -> 协程ID）
        private Dictionary<string, string> _loadingTasks = new Dictionary<string, string>();

        /// <summary>
        /// 同步加载FGUI包
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
        /// 异步加载FGUI包（通过协程管理器）
        /// </summary>
        public void AddPackageAsync(string packageName, System.Action onComplete = null)
        {
            if (_loadedPackages.Contains(packageName))
            {
                onComplete?.Invoke();
                return;
            }

            // 避免重复加载
            if (_loadingTasks.ContainsKey(packageName)) return;

            var coroutineId = CoroutineManager.Instance.StartManagedCoroutine(
                LoadPackageRoutine(packageName, onComplete)
            );
            _loadingTasks.Add(packageName, coroutineId);
        }

        /// <summary>
        /// 卸载包
        /// </summary>
        public void RemovePackage(string packageName)
        {
            if (_loadedPackages.Contains(packageName))
            {
                UIPackage.RemovePackage(packageName);
                _loadedPackages.Remove(packageName);
            }
        }

        // 异步加载协程
        private IEnumerator LoadPackageRoutine(string packageName, System.Action onComplete)
        {
            // 异步加载（实际加载逻辑需根据项目调整）
            yield return null;
            UIPackage.AddPackage(packageName);
            _loadedPackages.Add(packageName);
            _loadingTasks.Remove(packageName);
            onComplete?.Invoke();
        }
    }
}