using Animation;
using Com.Module.Chat;
using Com.Module.CommonResources;
using Com.Module.MainUI;
using Com.Module.Schedule;
using Core.Framework.Config;
using Core.Framework.Event;
using Core.Framework.FGUI;
using Core.Framework.Plugin;
using Core.Framework.Resource;
using FairyGUI;
using Module.chat;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    private Dictionary<Type, PluginBase> _plugins = new Dictionary<Type, PluginBase>();
    private string debugStr;
    private GameObject mikuInstance;

    private void Awake()
    {
        // 初始化 DesktopManager
        GameObject desktopManagerObj = new GameObject("DesktopManager");
        desktopManagerObj.AddComponent<DesktopManager>();

        // 初始化模块插件
        Install<ChatPlugin>();
        InitializeManagers();
        ApplyConfigurations();
        PreloadEssentialResources();

        // 生成 Miku
        SpawnMiku();
    }

    private void Update()
    {
        PluginsUpdate();

#if UNITY_EDITOR
        debugStr = GRoot._inst.touchTarget == null ? "null" : GRoot._inst.touchTarget.name.ToString();
#endif
    }

    private void OnGUI()
    {
#if UNITY_EDITOR
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, 1f, 1.0f));
        GUI.Label(new Rect(10, 10, 600, 370), debugStr, new GUIStyle() { fontSize = Math.Max(10, 30), normal = new GUIStyleState() { textColor = UnityEngine.Color.red } });
#endif
    }

    private void PluginsUpdate()
    {
        foreach (var plugin in _plugins.Values)
            plugin.Update();
    }

    private void InitializeManagers()
    {
        ConfigManager.Instance.Initialize();
    }

    private void ApplyConfigurations()
    {
        var networkConfig = ConfigManager.Instance.Network;
    }

    #region UI插件
    private void Install<T>() where T : PluginBase, new()
    {
        var plugin = new T();
        _plugins[typeof(T)] = plugin;
        plugin.Register();
    }

    private void Uninstall<T>() where T : PluginBase
    {
        if (_plugins.TryGetValue(typeof(T), out var plugin))
        {
            plugin.Uninstall();
            _plugins.Remove(typeof(T));
        }
    }

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
        ResourcesManager.Instance.FGUIResourceManager.AddPackageAsync("UI/CommonResources", () =>
        {
            Debug.Log("Common包加载完成！");
            if(ConfigManager.Instance.Game.IsFirstTimeOpen)
                GUIManager.Instance.ShowWindow<SettingWindow>();
#if UNITY_EDITOR
            GUIManager.Instance.ShowWindow<MainUIWindow>();
#endif
        });

        ChatBinder.BindAll();
        CommonResourcesBinder.BindAll();
        MainUIBinder.BindAll();
        ScheduleBinder.BindAll();
    }
    //linww后面换成通用的
    private void SpawnMiku()
    {
        GameObject mikuPrefab = Resources.Load<GameObject>("Model/Miku/miku");
        if (mikuPrefab == null)
        {
            Debug.LogError("Failed to load Miku prefab from Resources/Model/Miku!");
            return;
        }
        var pos = new Vector3(ConfigManager.Instance.UI.HorizontalOffset, ConfigManager.Instance.UI.VerticalOffset, 1);//进去一点
        mikuInstance = Instantiate(mikuPrefab, pos, Quaternion.identity);
        mikuInstance.name = "Miku";
        mikuInstance.transform.localScale = Vector3.one * ConfigManager.Instance.UI.UIScale;

        Animator animator = mikuInstance.GetComponent<Animator>() ?? mikuInstance.AddComponent<Animator>();
        AnimationManager animManager = mikuInstance.GetComponent<AnimationManager>() ?? mikuInstance.AddComponent<AnimationManager>();
        Pet petScript = mikuInstance.GetComponent<Pet>() ?? mikuInstance.AddComponent<Pet>();
    }

    private void OnDestroy()
    {
        ConfigManager.Instance.SaveAllConfigs();
        if (mikuInstance != null)
        {
            Destroy(mikuInstance);
        }
    }
}