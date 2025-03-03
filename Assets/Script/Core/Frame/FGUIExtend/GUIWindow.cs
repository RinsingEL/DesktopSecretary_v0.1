// GUIWindow.cs
using FairyGUI;
using UnityEngine;
using Core.Framework.Resource;

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
            public string packagePath;  // FGUI包路径
            public string packageName;  // FGUI包名
            public string componentName; // 组件名
            public bool IsFullScreen;
            public bool IsFit;
            public UILayer Layer;
            public bool CloseOnClickOutside; // 点击外部关闭
        }

        public GUIParam Param;
        public GComponent _root;     // 窗口根组件
        private bool _isShown;          // 是否已显示
        private bool _initialized = false;
        #endregion

        #region 生命周期
        public GUIWindow()
        {
            Param = new GUIParam
            {
                IsFullScreen = true,
                IsFit = true,
                Layer = UILayer.Normal,
                CloseOnClickOutside = false
            };
        }

        /// <summary>
        /// 初始化窗口（必须调用）
        /// </summary>
        public void Initialize()
        {
            if (_initialized) return;
            // 加载资源包
            ResourcesManager.Instance.FGUIResourceManager.AddPackage(Param.packagePath);

            var Type = GetType();

            if (GUIManager.Instance._hiddenWindows.ContainsKey(Type))
                _root = GUIManager.Instance._hiddenWindows[Type]._root;
            else
                _root = UIPackage.CreateObject(Param.packageName, Param.componentName).asCom;
            if (_root == null)
            {
                Debug.LogError($"无法从 [{Param.packageName}] 加载 [{Param.componentName}]");
                return;
            }



            // 全屏处理
            if (Param.IsFullScreen)
            {
                _root.SetSize(GRoot.inst.width, GRoot.inst.height);
            }

            // 初始化组件 
            OnInit(_root);
            // 居中显示
            Center();
            _initialized = true;
        }
        public virtual void InitializeParam(ShowWindowParam param)
        {
            
        }

        /// <summary>
        /// 子类实现的初始化方法
        /// </summary>
        protected abstract void OnInit(GComponent com);

        /// <summary>
        /// 销毁窗口
        /// </summary>
        public void Dispose()
        {
            Hide();
            _root.Dispose();
            this.Dispose();
        }
        #endregion

        #region 显示控制
        public void Show()
        {

            if (!_initialized) Initialize();

            var container = GUIManager.Instance.GetLayerContainer(Param.Layer);
            if (container != null)
            {
                container.AddChild(_root);
                GUIManager.Instance.AddActiveWindow(this);
                _isShown = true;
            }

            if (Param.IsFit) GUIManager.Instance.AutoFitWindow(this);

            OnShow();
        }

        public void Hide()
        {
            if (!_isShown) return;


            _root.RemoveFromParent();
            GUIManager.Instance.MoveToHiddenWindows(this);
            _isShown = false;
            OnHide();
        }
        #endregion

        #region 内部方法

        public void Center()
        {
            _root.SetXY((GRoot.inst.width - _root.scaleX * _root.sourceWidth) / 2,
                (GRoot.inst.height - _root.scaleY * _root.sourceHeight) / 2);//不知道SetScale这玩意到底是在哪个生命周期里的，怎么半天不变的（恼）
        }

        private void OnModalClick()
        {
            if (Param.CloseOnClickOutside)
            {
                Hide();
            }
        }
        #endregion

        #region 可重写方法
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
        #endregion
        public abstract class ShowWindowParam
        {
            public ShowWindowParam()
            {
            }
        }
    }
}