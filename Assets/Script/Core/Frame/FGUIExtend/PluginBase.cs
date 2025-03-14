// PluginBase.cs
using System;
using UnityEngine.EventSystems;

namespace Core.Framework.Plugin
{
    /// <summary>
    /// 插件基类，记得写完在StartGame注册
    /// </summary>
    public abstract class PluginBase
    {
        // 是否已初始化
        private bool _isInitialized = false;

        public void Register()
        {
            if (_isInitialized) return;
            OnRegister();
            _isInitialized = true;
        }
        public void Update()
        {
            OnUpdate();//在主脚本里面用，还是老实用unity生命周期管理了
        }
        /// <summary>
        /// 卸载插件
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