using System.Collections.Generic;
using FairyGUI;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

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

        #region 窗口管理
        public T ShowWindow<T>() where T : GUIWindow, new()
        {
            System.Type type = typeof(T);

            // 优先从隐藏列表恢复
            if (_hiddenWindows.TryGetValue(type, out var hiddenWindow))
            {
                _hiddenWindows.Remove(type);
                _activeWindows[type] = hiddenWindow;
                hiddenWindow.Show();
                return (T)hiddenWindow;
            }

            // 创建新窗口
            if (!_allWindows.ContainsKey(type))
            {
                var newWindow = new T();
                newWindow.Initialize();
                _allWindows.Add(type, newWindow);
                _activeWindows.Add(type, newWindow);
                newWindow.Show();
                return newWindow;
            }

            // 已存在且处于显示状态
            if (_activeWindows.ContainsKey(type))
            {
                Debug.LogWarning($"窗口 [{type.Name}] 已经处于显示状态");
                return (T)_activeWindows[type];
            }

            // 异常情况处理
            Debug.LogError($"窗口 [{type.Name}] 处于未知状态");
            return null;
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
    }
}