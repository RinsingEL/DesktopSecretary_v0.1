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
        #region ��������
        public struct GUIParam
        {
            public string packagePath;  // FGUI��·��
            public string packageName;  // FGUI����
            public string componentName; // �����
            public bool IsFullScreen;
            public bool IsFit;
            public UILayer Layer;
            public bool CloseOnClickOutside; // ����ⲿ�ر�
        }

        public GUIParam Param;
        public GComponent _root;     // ���ڸ����
        private bool _isShown;          // �Ƿ�����ʾ
        private bool _initialized = false;
        #endregion

        #region ��������
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
        /// ��ʼ�����ڣ�������ã�
        /// </summary>
        public void Initialize()
        {
            if (_initialized) return;
            // ������Դ��
            ResourcesManager.Instance.FGUIResourceManager.AddPackage(Param.packagePath);

            var Type = GetType();

            if (GUIManager.Instance._hiddenWindows.ContainsKey(Type))
                _root = GUIManager.Instance._hiddenWindows[Type]._root;
            else
                _root = UIPackage.CreateObject(Param.packageName, Param.componentName).asCom;
            if (_root == null)
            {
                Debug.LogError($"�޷��� [{Param.packageName}] ���� [{Param.componentName}]");
                return;
            }



            // ȫ������
            if (Param.IsFullScreen)
            {
                _root.SetSize(GRoot.inst.width, GRoot.inst.height);
            }

            // ��ʼ����� 
            OnInit(_root);
            // ������ʾ
            Center();
            _initialized = true;
        }
        public virtual void InitializeParam(ShowWindowParam param)
        {
            
        }

        /// <summary>
        /// ����ʵ�ֵĳ�ʼ������
        /// </summary>
        protected abstract void OnInit(GComponent com);

        /// <summary>
        /// ���ٴ���
        /// </summary>
        public void Dispose()
        {
            Hide();
            _root.Dispose();
            this.Dispose();
        }
        #endregion

        #region ��ʾ����
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

        #region �ڲ�����

        public void Center()
        {
            _root.SetXY((GRoot.inst.width - _root.scaleX * _root.sourceWidth) / 2,
                (GRoot.inst.height - _root.scaleY * _root.sourceHeight) / 2);//��֪��SetScale�����⵽�������ĸ�����������ģ���ô���첻��ģ��գ�
        }

        private void OnModalClick()
        {
            if (Param.CloseOnClickOutside)
            {
                Hide();
            }
        }
        #endregion

        #region ����д����
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