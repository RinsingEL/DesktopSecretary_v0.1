using Com.Module.Chat;
using Com.Module.CommonResources;
using Com.Module.MainUI;
using Com.Module.Schedule;
using Core.Framework.Config;
using Core.Framework.FGUI;
using Core.Framework.Plugin;
using Core.Framework.Resource;
using Module.chat;
using System;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    // 插件实例字典（类型 -> 实例）
    private Dictionary<Type, PluginBase> _plugins = new Dictionary<Type, PluginBase>();
    private void Awake()
    {
        // 初始化模块插件
        Install<ChatPlugin>();
        Install<SchedulePlugin>();
        InitializeManagers();
        ApplyConfigurations();
        PreloadEssentialResources();
    }

    private void InitializeManagers()
    {
        // 非Mono系管理器
        ConfigManager.Instance.Initialize();
    }

    private void ApplyConfigurations()
    {
        // 应用网络配置
        var networkConfig = ConfigManager.Instance.Network;
    }
    #region UI插件
    /// <summary>
    /// 安装插件
    /// </summary>
    private void Install<T>() where T : PluginBase, new()
    {
        var plugin = new T();
        _plugins[typeof(T)] = plugin;
        plugin.Register();
    }
    /// <summary>
    /// 卸载插件
    /// </summary>
    private void Uninstall<T>() where T : PluginBase
    {
        if (_plugins.TryGetValue(typeof(T), out var plugin))
        {
            plugin.Uninstall();
            _plugins.Remove(typeof(T));
        }
    }
    /// <summary>
    /// 卸载所有插件
    /// </summary>
    private void UninstallAll()
    {
        foreach (var plugin in _plugins.Values)
        {
            plugin.Uninstall();
        }
        _plugins.Clear();
    }
    #endregion
    private void PreloadEssentialResources()
    {
        // 预加载必要的UI包（如Common包）
        ResourcesManager.Instance.FGUIResourceManager.AddPackageAsync("UI/CommonResources", () =>
        {
            Debug.Log("Common包加载完成！");
            
            //第一次打开则打开设置界面,待会再写
            GUIManager.Instance.ShowWindow<SettingWindow>();
            GUIManager.Instance.ShowWindow<MainUIWindow>();
            // 加载完成后打开主界面


        });
        //加载其他UI包
        ChatBinder.BindAll();
        CommonResourcesBinder.BindAll();
        MainUIBinder.BindAll();
        ScheduleBinder.BindAll();
    }
    private void OnDestroy()
    {
        ConfigManager.Instance.SaveAllConfigs();
    }
}