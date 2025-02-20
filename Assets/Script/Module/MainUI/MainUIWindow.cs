using Com.Module.MainUI;
using Core.Framework.FGUI;
using FairyGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUIWindow : GUIWindow
{
    public UI_MainWindow rootwindow;
    public MainUIWindow()
    {
        Param.packagePath = "UI/MainUI";
        Param.packageName = "MainUI";
        Param.componentName = "MainWindow";
        Param.Layer = UILayer.Main;
    }
    protected override void OnInit(GComponent com)
    {
        rootwindow = com as UI_MainWindow;
        rootwindow.m_MenuList.Init();

    }
}
