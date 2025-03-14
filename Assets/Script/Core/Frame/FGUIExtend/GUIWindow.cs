using FairyGUI;
using UnityEngine;
using Core.Framework.Resource;
using Core.Framework.Event;
using Core.Framework.Utility;

namespace Core.Framework.FGUI
{
    public enum UILayer
    {
        Background,
        Normal,
        Main,
        Popup,
        Loading,
        Top
    }

    public abstract class GUIWindow
    {
        #region 窗口配置
        public struct GUIParam
        {
            public string packagePath;
            public string packageName;
            public string componentName;
            public bool IsFullScreen;
            public bool IsFit;
            public UILayer Layer;
            public bool CloseOnClickOutside;
        }

        public GUIParam Param;
        public GComponent _root;
        private bool _isShown;
        private bool _initialized = false;
        private bool _isDataReady = false;
        private bool _isOutside = true;
        #endregion

        #region 生命周期
        public GUIWindow()
        {
            Param = new GUIParam
            {
                IsFullScreen = true,
                IsFit = true,
                Layer = UILayer.Normal,
                CloseOnClickOutside = true
            };
        }

        public System.Collections.IEnumerator InitializeAsync()
        {
            if (_initialized) yield break;

            ResourcesManager.Instance.FGUIResourceManager.AddPackage(Param.packagePath);

            var type = GetType();
            if (GUIManager.Instance._hiddenWindows.ContainsKey(type))
                _root = GUIManager.Instance._hiddenWindows[type]._root;
            else
                _root = UIPackage.CreateObject(Param.packageName, Param.componentName).asCom;

            if (_root == null)
            {
                Debug.LogError($"无法从 [{Param.packageName}] 加载 [{Param.componentName}]");
                yield break;
            }

            if (Param.IsFullScreen)
            {
                _root.SetSize(GRoot.inst.width, GRoot.inst.height);
            }

            // 调用子类的异步初始化逻辑
            OnInit(_root);
            Center();
            _initialized = true;
            _isDataReady = true; 
        }

        public virtual void InitializeParam(ShowWindowParam param) { }

        protected abstract void OnInit(GComponent com);
        public void Destroy()
        {
            if (_root == null) return;

            if (_isShown)
            {
                Hide();
            }

            if (!_root.isDisposed)
            {
                _root.Dispose();
            }
            _root = null;

            GUIManager.Instance.RemoveWindow(this);
            OnDestroy();
        }
        #endregion

        #region 显示控制
        public System.Collections.IEnumerator ShowAsync()
        {
            if (!_initialized)
            {
                yield return CoroutineManager.Instance.StartManagedCoroutine(InitializeAsync());
            }

            if (!_isDataReady)
            {
                yield return new WaitUntil(() => _isDataReady);
            }

            BeforeShow();
            var container = GUIManager.Instance.GetLayerContainer(Param.Layer);
            if (container != null)
            {
                container.AddChild(_root);
                GUIManager.Instance.AddActiveWindow(this);
                _isShown = true;

                if (Param.CloseOnClickOutside)
                {
                    Stage.inst.onClick.Set(OnModalClickHandler);
                    _root.onRollOut.Set(() => { _isOutside = true; });
                    _root.onRollOver.Set(() => { _isOutside = false; });
                }
            }

            if (Param.IsFit) GUIManager.Instance.AutoFitWindow(this);
            OnShow();
        }

        public void Show()
        {
            CoroutineManager.Instance.StartManagedCoroutine(ShowAsync());
        }

        public void Hide()
        {
            if (!_isShown || _root == null) return;

            if (Param.CloseOnClickOutside)
            {
                Stage.inst.onClick.Remove(OnModalClickHandler);
                _root.onRollOut.Clear();
                _root.onRollOver.Clear();
            }

            Debug.Log($"隐藏 {GetType().Name}");
            _root.RemoveFromParent();
            GUIManager.Instance.MoveToHiddenWindows(this);
            _isShown = false;
            _initialized = false;
            _isDataReady = false;
            OnHide();
        }
        #endregion

        #region 内部方法
        public void Center()
        {
            if (_root != null && !_root.isDisposed)
            {
                _root.SetXY((GRoot.inst.width - _root.scaleX * _root.sourceWidth) / 2,
                            (GRoot.inst.height - _root.scaleY * _root.sourceHeight) / 2);
            }
        }

        private void OnModalClickHandler()
        {

            if (Param.CloseOnClickOutside && _root != null && !_root.isDisposed)
            {
                if (_isOutside)
                {
                    Hide();
                }
            }
        }
        #endregion

        #region 可重写方法
        protected virtual void BeforeShow() { }
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
        protected virtual void OnDestroy() { }
        #endregion

        public abstract class ShowWindowParam
        {
            public ShowWindowParam() { }
        }
    }
}