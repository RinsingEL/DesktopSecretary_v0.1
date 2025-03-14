using Core.Framework.Utility;
using FairyGUI;
using System;
using System.Collections;
using UnityEngine;
using static Com.Module.Chat.DialoguePanel;

namespace Com.Module.Chat
{
    public partial class UI_DialoguePanel : GComponent
    {
        private string dialogue;
        private Action hide;
        public void Init(DialogueParam param)
        {
            dialogue = param.dialogue;
            hide = param.hide;
        }
        public void UpdateView()
        {
            CoroutineManager.Instance.StartManagedCoroutine(UpdateStrByStep());
        }
        public IEnumerator UpdateStrByStep()
        {
            if (dialogue == string.Empty)
            {
                yield break;
            }

            string currentText = "";
            float charDelay = 0.2f;

            // Öð¸ö×Ö·ûÏÔÊ¾
            for (int i = 0; i < dialogue.Length; i++)
            {
                currentText += dialogue[i];
                m_dialogueTxt.text = currentText;
                yield return new WaitForSeconds(charDelay);
            }

            yield return new WaitForSeconds(5f);
            hide();
        }
    }
}