using Core.Framework.Pet;
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
        string coru = "";
        string speekCoru = "";
        public void Init(DialogueParam param)
        {
            dialogue = param.dialogue;
            hide = () => { param.hide();
                CoroutineManager.Instance.StopManagedCoroutine(speekCoru);
                Pet.Instance.CloseMouth();
            };;
        }
        public void UpdateView()
        {
            if (coru != string.Empty)
                CoroutineManager.Instance.StopManagedCoroutine(coru);
           coru =  CoroutineManager.Instance.StartManagedCoroutine(UpdateStrByStep());

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
                if (i == 0)
                {
                   speekCoru = CoroutineManager.Instance.StartManagedCoroutine(Pet.Instance.Speek());
                }
                yield return new WaitForSeconds(charDelay);
            }

            CoroutineManager.Instance.StopManagedCoroutine(speekCoru);
            Pet.Instance.CloseMouth();

            yield return new WaitForSeconds(0.5f * dialogue.Length);
            hide();
            Pet.Instance.CloseMouth();
        }
    }
}