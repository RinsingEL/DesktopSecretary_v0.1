using System;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

namespace Core.Framework.FGUI
{
    public class GUIManager : MonoBehaviour
    {
        private static GUIManager _instance;
        public static GUIManager Instance => _instance ??= CreateInstance();

        // 窗口存储结构
        public Dictionary<System.Type, GUIWindow> _allWindows = new Dictionary<System.Type, GUIWindow>();
        public Dictionary<System.Type, GUIWindow> _activeWindows = new Dictionary<System.Type, GUIWindow>();
        public Dictionary<System.Type, GUIWindow> _hiddenWindows = new Dictionary<System.Type, GUIWindow>();

        // 层级容器
        private Dictionary<UILayer, GComponent> _layers = new Dictionary<UILayer, GComponent>();
        //窗口跟随
        private Dictionary<GUIWindow, GameObject> _windowFollowTargets = new Dictionary<GUIWindow, GameObject>();
        private Dictionary<GUIWindow, Vector2> _windowOffsets = new Dictionary<GUIWindow, Vector2>();

        #region 初始化
        private static GUIManager CreateInstance()
        {
            var go = new GameObject("GUIManager");
            DontDestroyOnLoad(go);
            var instance = go.AddComponent<GUIManager>();
            instance.InitializeLayers();
            return instance;
        }

        private void InitializeLayers()
        {
            CreateLayer(UILayer.Background, 100);
            CreateLayer(UILayer.Main, 200);
            CreateLayer(UILayer.Normal, 300);
            CreateLayer(UILayer.Popup, 400);
            CreateLayer(UILayer.Loading, 500);
            CreateLayer(UILayer.Top, 600);

            GRoot.inst.onSizeChanged.Add(() =>
            {
                foreach (var window in _activeWindows.Values)
                    if (window.Param.IsFit) AutoFitWindow(window);
            });
        }

        private void CreateLayer(UILayer layerType, int sortingOrder)
        {
            var layer = new GComponent();
            layer.gameObjectName = $"{layerType}";
            GRoot.inst.AddChild(layer);
            layer.sortingOrder = sortingOrder;
            layer.SetSize(GRoot.inst.width, GRoot.inst.height);
            layer.AddRelation(GRoot.inst, RelationType.Size);
            _layers.Add(layerType, layer);
        }
        #endregion
        private void Update()
        {
            //实时跟随窗口
            foreach (var window in _windowFollowTargets.Keys)
            {
                UpdateWindowPosition(window);
            }
        }
        #region 窗口管理
        public void ShowWindow<T>() where T : GUIWindow, new()
        {
            System.Type type = typeof(T);

            // 优先从隐藏列表恢复
            if (_hiddenWindows.TryGetValue(type, out var hiddenWindow))
            {
                _hiddenWindows.Remove(type);
                _activeWindows[type] = hiddenWindow;
                hiddenWindow.Show();
                return;
            }

            // 创建新窗口
            if (!_allWindows.ContainsKey(type))
            {
                var newWindow = new T();
                _allWindows.Add(type, newWindow);
                _activeWindows.Add(type, newWindow);
                newWindow.Show(); // 异步显示
                return;
            }

            // 已存在且处于显示状态
            if (_activeWindows.ContainsKey(type))
            {
                Debug.LogWarning($"窗口 [{type.Name}] 已经处于显示状态");
                return;
            }

            Debug.LogError($"窗口 [{type.Name}] 处于未知状态");
        }

        public void ShowWindow(GUIWindow.ShowWindowParam showWindowParam)
        {
            if (showWindowParam == null)
            {
                Debug.LogError("参数是空的");
                return;
            }

            Type windowType = showWindowParam.GetType().DeclaringType;
            if (windowType == null || !typeof(GUIWindow).IsAssignableFrom(windowType))
            {
                Debug.LogError("检查参数的窗口类型");
                return;
            }

            // 检查隐藏窗口
            if (_hiddenWindows.TryGetValue(windowType, out var hiddenWindow))
            {
                _hiddenWindows.Remove(windowType);
                _activeWindows[windowType] = hiddenWindow;
                hiddenWindow.Show();
                return;
            }

            // 检查现有窗口
            if (_allWindows.TryGetValue(windowType, out var existingWindow))
            {
                if (_activeWindows.ContainsKey(windowType))
                {
                    Debug.LogWarning($"窗口 [{windowType.Name}] 已经处于显示状态");
                    return;
                }
            }

            // 创建新窗口
            try
            {
                GUIWindow newWindow = (GUIWindow)Activator.CreateInstance(windowType);
                if (newWindow != null)
                {
                    newWindow.InitializeParam(showWindowParam);
                    _allWindows[windowType] = newWindow;
                    _activeWindows[windowType] = newWindow;
                    newWindow.Show(); // 异步显示
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"创建窗口 [{windowType.Name}] 失败: {e.Message}");
                return;
            }

            Debug.LogError($"窗口 [{windowType.Name}] 创建失败");
        }


        public void AddActiveWindow(GUIWindow window)
        {
            var type = window.GetType();
            if (_activeWindows.ContainsKey(type)) return;

            _activeWindows.Add(type, window);
            if (_hiddenWindows.ContainsKey(type))
                _hiddenWindows.Remove(type);
        }

        public void MoveToHiddenWindows(GUIWindow window)
        {
            var type = window.GetType();
            if (!_activeWindows.ContainsKey(type)) return;

            _activeWindows.Remove(type);
            _hiddenWindows[type] = window;
        }

        public void RemoveWindow(GUIWindow window)
        {
            var type = window.GetType();
            _allWindows.Remove(type);
            _activeWindows.Remove(type);
            _hiddenWindows.Remove(type);
            StopWindowFollow(window);
        }

        public void HideWindow<T>() where T : GUIWindow
        {
            if (_activeWindows.TryGetValue(typeof(T), out var window))
            {
                window.Hide();
            }
        }
        #endregion

        #region 公共方法
        public GComponent GetLayerContainer(UILayer layer)
        {
            return _layers.TryGetValue(layer, out var container) ? container : null;
        }

        public void AutoFitWindow(GUIWindow window)
        {
            if (window == null || window._root == null) return;

            Vector2 designSize = new Vector2(1920, 1080);
            Vector2 currentSize = new Vector2(GRoot.inst.width, GRoot.inst.height);

            float scaleRatio = Mathf.Min(
                currentSize.x / designSize.x,
                currentSize.y / designSize.y
            );

            window._root.SetScale(scaleRatio, scaleRatio);
            window.Center();
        }

        public T GetWindow<T>() where T : GUIWindow
        {
            return _allWindows.TryGetValue(typeof(T), out var window) ? (T)window : null;
        }
        #endregion
        #region  窗口跟随UI
        /// <summary>
        /// 设置窗口跟随某个GameObject
        /// </summary>
        /// <param name="window">要跟随的窗口</param>
        /// <param name="target">目标GameObject</param>
        /// <param name="offset">相对于目标的偏移量（可选）</param>
        public void SetWindowFollow(GUIWindow window, GameObject target, Vector2 offset = default)
        {
            if (window == null || target == null)
            {
                Debug.LogError("Window or target cannot be null");
                return;
            }

            // 存储跟随关系
            _windowFollowTargets[window] = target;
            _windowOffsets[window] = offset;

            // 确保窗口已添加到活动窗口
            AddActiveWindow(window);

            // 更新
            UpdateWindowPosition(window);
        }

        /// <summary>
        /// 停止窗口跟随
        /// </summary>
        /// <param name="window">要停止跟随的窗口</param>
        public void StopWindowFollow(GUIWindow window)
        {
            if (window != null)
            {
                _windowFollowTargets.Remove(window);
                _windowOffsets.Remove(window);
            }
        }

        /// <summary>
        /// 更新窗口位置以跟随目标
        /// </summary>
        private void UpdateWindowPosition(GUIWindow window)
        {
            if (!_windowFollowTargets.TryGetValue(window, out GameObject target) || target == null)
                return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(target.transform.position);
            // 转换为FairyGUI坐标系
            Vector2 uiPos = GRoot.inst.GlobalToLocal(new Vector2(screenPos.x, Screen.height - screenPos.y));
            // 应用偏移
            uiPos += _windowOffsets.TryGetValue(window, out Vector2 offset) ? offset : Vector2.zero;

            // 设置窗口位置
            window._root.SetPosition(uiPos.x, uiPos.y , 0);//不需要Z轴目前
        }
        #endregion
    }
}