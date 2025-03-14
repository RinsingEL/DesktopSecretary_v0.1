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

        // ���ڴ洢�ṹ
        public Dictionary<System.Type, GUIWindow> _allWindows = new Dictionary<System.Type, GUIWindow>();
        public Dictionary<System.Type, GUIWindow> _activeWindows = new Dictionary<System.Type, GUIWindow>();
        public Dictionary<System.Type, GUIWindow> _hiddenWindows = new Dictionary<System.Type, GUIWindow>();

        // �㼶����
        private Dictionary<UILayer, GComponent> _layers = new Dictionary<UILayer, GComponent>();
        //���ڸ���
        private Dictionary<GUIWindow, GameObject> _windowFollowTargets = new Dictionary<GUIWindow, GameObject>();
        private Dictionary<GUIWindow, Vector2> _windowOffsets = new Dictionary<GUIWindow, Vector2>();

        #region ��ʼ��
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
            //ʵʱ���洰��
            foreach (var window in _windowFollowTargets.Keys)
            {
                UpdateWindowPosition(window);
            }
        }
        #region ���ڹ���
        public void ShowWindow<T>() where T : GUIWindow, new()
        {
            System.Type type = typeof(T);

            // ���ȴ������б�ָ�
            if (_hiddenWindows.TryGetValue(type, out var hiddenWindow))
            {
                _hiddenWindows.Remove(type);
                _activeWindows[type] = hiddenWindow;
                hiddenWindow.Show();
                return;
            }

            // �����´���
            if (!_allWindows.ContainsKey(type))
            {
                var newWindow = new T();
                _allWindows.Add(type, newWindow);
                _activeWindows.Add(type, newWindow);
                newWindow.Show(); // �첽��ʾ
                return;
            }

            // �Ѵ����Ҵ�����ʾ״̬
            if (_activeWindows.ContainsKey(type))
            {
                Debug.LogWarning($"���� [{type.Name}] �Ѿ�������ʾ״̬");
                return;
            }

            Debug.LogError($"���� [{type.Name}] ����δ֪״̬");
        }

        public void ShowWindow(GUIWindow.ShowWindowParam showWindowParam)
        {
            if (showWindowParam == null)
            {
                Debug.LogError("�����ǿյ�");
                return;
            }

            Type windowType = showWindowParam.GetType().DeclaringType;
            if (windowType == null || !typeof(GUIWindow).IsAssignableFrom(windowType))
            {
                Debug.LogError("�������Ĵ�������");
                return;
            }

            // ������ش���
            if (_hiddenWindows.TryGetValue(windowType, out var hiddenWindow))
            {
                _hiddenWindows.Remove(windowType);
                _activeWindows[windowType] = hiddenWindow;
                hiddenWindow.Show();
                return;
            }

            // ������д���
            if (_allWindows.TryGetValue(windowType, out var existingWindow))
            {
                if (_activeWindows.ContainsKey(windowType))
                {
                    Debug.LogWarning($"���� [{windowType.Name}] �Ѿ�������ʾ״̬");
                    return;
                }
            }

            // �����´���
            try
            {
                GUIWindow newWindow = (GUIWindow)Activator.CreateInstance(windowType);
                if (newWindow != null)
                {
                    newWindow.InitializeParam(showWindowParam);
                    _allWindows[windowType] = newWindow;
                    _activeWindows[windowType] = newWindow;
                    newWindow.Show(); // �첽��ʾ
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"�������� [{windowType.Name}] ʧ��: {e.Message}");
                return;
            }

            Debug.LogError($"���� [{windowType.Name}] ����ʧ��");
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

        #region ��������
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
        #region  ���ڸ���UI
        /// <summary>
        /// ���ô��ڸ���ĳ��GameObject
        /// </summary>
        /// <param name="window">Ҫ����Ĵ���</param>
        /// <param name="target">Ŀ��GameObject</param>
        /// <param name="offset">�����Ŀ���ƫ��������ѡ��</param>
        public void SetWindowFollow(GUIWindow window, GameObject target, Vector2 offset = default)
        {
            if (window == null || target == null)
            {
                Debug.LogError("Window or target cannot be null");
                return;
            }

            // �洢�����ϵ
            _windowFollowTargets[window] = target;
            _windowOffsets[window] = offset;

            // ȷ����������ӵ������
            AddActiveWindow(window);

            // ����
            UpdateWindowPosition(window);
        }

        /// <summary>
        /// ֹͣ���ڸ���
        /// </summary>
        /// <param name="window">Ҫֹͣ����Ĵ���</param>
        public void StopWindowFollow(GUIWindow window)
        {
            if (window != null)
            {
                _windowFollowTargets.Remove(window);
                _windowOffsets.Remove(window);
            }
        }

        /// <summary>
        /// ���´���λ���Ը���Ŀ��
        /// </summary>
        private void UpdateWindowPosition(GUIWindow window)
        {
            if (!_windowFollowTargets.TryGetValue(window, out GameObject target) || target == null)
                return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(target.transform.position);
            // ת��ΪFairyGUI����ϵ
            Vector2 uiPos = GRoot.inst.GlobalToLocal(new Vector2(screenPos.x, Screen.height - screenPos.y));
            // Ӧ��ƫ��
            uiPos += _windowOffsets.TryGetValue(window, out Vector2 offset) ? offset : Vector2.zero;

            // ���ô���λ��
            window._root.SetPosition(uiPos.x, uiPos.y , 0);//����ҪZ��Ŀǰ
        }
        #endregion
    }
}