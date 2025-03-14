using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using Core.Framework.Network;
using Core.Framework.Event;

public class TestClass : MonoBehaviour
{
    ChatViewModel chatViewModel;
    // Start is called before the first frame update
    void Start()
    {
        chatViewModel = new ChatViewModel();
        string msg = "麻烦帮我看看2025年1月和开会有关的记录呗";
        EventManager.Instance.Trigger<string, string>(ClientEvent.ON_SEND_FUNC_REQUEST, msg, "generateCrudQuery");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
