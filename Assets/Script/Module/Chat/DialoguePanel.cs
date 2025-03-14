using Com.Module.Chat;
using Core.Framework.FGUI;
using FairyGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Camera;

namespace Com.Module.Chat
{
    public class DialoguePanel : GUIWindow
    {
        public UI_DialoguePanel rootwindow;
        public DialogueParam param;

        public DialoguePanel()
        {
            Param.packageName = "Chat";
            Param.componentName = "DialoguePanel";
            Param.packagePath = "UI/Chat";
            Param.Layer = UILayer.Normal;
        }
        protected override void OnInit(GComponent com)
        {
            rootwindow = com as UI_DialoguePanel;
            rootwindow.Init(param);
            GUIManager.Instance.SetWindowFollow(this, Pet.Instance.gameObject, new Vector2( 50 , 50));
        }
        public override void InitializeParam(ShowWindowParam param)
        {
            base.InitializeParam(param);
            this.param = param as DialogueParam;
            this.param.hide = Hide;
        }
        protected override void BeforeShow()
        {
            rootwindow.UpdateView();
        }
        public class DialogueParam : ShowWindowParam
        {
            public string dialogue;
            public Action hide;
        }
    }

}

