using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

public class TestClass : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Window window = new Window();
        UIPackage.AddPackage("UI/Chat");
        var pack = UIPackage.GetByName("UI/Chat");
        var panel = UIPackage.CreateObject("Chat", "MenuWindow");
        if (panel != null)
            window.contentPane = panel as GComponent;
        window.Show();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
