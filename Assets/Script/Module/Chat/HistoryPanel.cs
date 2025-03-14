using Com.Module.Chat;
using Core.Framework.FGUI;
using FairyGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChatData;

public class HistoryPanel : GUIWindow
{
    private UI_HistoryPanel rootWindow;
    public HistoryParam param;
    public HistoryPanel()
    {
        Param.packagePath = "UI/Chat";
        Param.packageName = "Chat";
        Param.componentName = "HistoryPanel";
        Param.Layer = UILayer.Normal;
    }
    public override void InitializeParam(ShowWindowParam param)
    {
        base.InitializeParam(param);
        this.param = param as HistoryParam;
    }
    protected override void OnInit(GComponent com)
    {
        rootWindow = com as UI_HistoryPanel;
        rootWindow.Init(param);
    }

    protected override void BeforeShow()
    {
        base.BeforeShow();
        rootWindow.BeforeShow();
    }
    public class HistoryParam : ShowWindowParam
    {
        public List<ChatMessage> History;
    }
}
