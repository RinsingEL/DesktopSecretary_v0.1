using Core.Framework.Event;
using FairyGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Module.Chat
{
    public partial class UI_HistoryPanel : GComponent
    {
        HistoryPanel.HistoryParam param;
        public void Init(HistoryPanel.HistoryParam param)
        {
            this.param = param;
            m_list_history.itemRenderer = OnRenderItem;
        }

        private void OnRenderItem(int index, GObject item)
        {
            var dialog = item as UI_dialogCom;
            if (param.History[index].Role == "user")
            {
                dialog.m_from.selectedIndex = 0;
                //¼ÓÔØÍ¼±ê
                dialog.m_userTxt.text = param.History[index].Content;
            }
            else
            {
                dialog.m_from.selectedIndex = 1;
                dialog.m_AITxt.text = param.History[index].Content;
            }
        }
        public void BeforeShow()
        {
            if(param.History.Count > 0)
            m_list_history.numItems = param.History.Count;
        }
    }
}
