// PluginBase.cs
using System;
using UnityEngine.EventSystems;

namespace Core.Framework.Plugin
{
    /// <summary>
    /// ������࣬�ǵ�д����StartGameע��
    /// </summary>
    public abstract class PluginBase
    {
        // �Ƿ��ѳ�ʼ��
        private bool _isInitialized = false;

        public void Register()
        {
            if (_isInitialized) return;
            OnRegister();
            _isInitialized = true;
        }
        public void Update()
        {
            OnUpdate();//�����ű������ã�������ʵ��unity�������ڹ�����
        }
        /// <summary>
        /// ж�ز��
        /// </summary>
        public void Uninstall()
        {
            if (!_isInitialized) return;
            OnUninstall();
            _isInitialized = false;
        }
        protected abstract void OnRegister();
        protected abstract void OnUpdate();
        protected abstract void OnUninstall();
    }
}