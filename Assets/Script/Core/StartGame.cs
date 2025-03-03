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
    // ���ʵ���ֵ䣨���� -> ʵ����
    private Dictionary<Type, PluginBase> _plugins = new Dictionary<Type, PluginBase>();
    private void Awake()
    {
        // ��ʼ��ģ����
        Install<ChatPlugin>();
        Install<SchedulePlugin>();
        InitializeManagers();
        ApplyConfigurations();
        PreloadEssentialResources();
    }

    private void InitializeManagers()
    {
        // ��Monoϵ������
        ConfigManager.Instance.Initialize();
    }

    private void ApplyConfigurations()
    {
        // Ӧ����������
        var networkConfig = ConfigManager.Instance.Network;
    }
    #region UI���
    /// <summary>
    /// ��װ���
    /// </summary>
    private void Install<T>() where T : PluginBase, new()
    {
        var plugin = new T();
        _plugins[typeof(T)] = plugin;
        plugin.Register();
    }
    /// <summary>
    /// ж�ز��
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
    /// ж�����в��
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
        // Ԥ���ر�Ҫ��UI������Common����
        ResourcesManager.Instance.FGUIResourceManager.AddPackageAsync("UI/CommonResources", () =>
        {
            Debug.Log("Common��������ɣ�");
            
            //��һ�δ�������ý���,������д
            GUIManager.Instance.ShowWindow<SettingWindow>();
            GUIManager.Instance.ShowWindow<MainUIWindow>();
            // ������ɺ��������


        });
        //��������UI��
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