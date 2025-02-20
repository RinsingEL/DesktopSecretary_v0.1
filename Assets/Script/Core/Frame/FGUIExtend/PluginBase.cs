// PluginBase.cs
using System;
using UnityEngine.EventSystems;

namespace Core.Framework.Plugin
{
    /// <summary>
    /// Plugin 基类，不继承 MonoBehaviour，提供单例模式、事件交互和生命周期管理
    /// </summary>
    public abstract class PluginBase
    {
        // 是否已初始化
        private bool _isInitialized = false;

        /// <summary>
        /// 注册插件（手动调用或通过依赖管理器触发）
        /// </summary>
        public void Register()
        {
            if (_isInitialized) return;
            OnRegister();
            _isInitialized = true;
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

        //--------------------------------------------------
        // 虚方法（子类实现）
        //--------------------------------------------------
        /// <summary>
        /// 注册事件、初始化数据等
        /// </summary>
        protected abstract void OnRegister();

        /// <summary>
        /// 清理事件、释放资源等
        /// </summary>
        protected abstract void OnUninstall();
    }
}