using Animation;
using Core.Framework.Event;
using Core.Framework.FGUI;
using Core.Framework.Network;
using Core.Framework.Network.ChatSystem;
using FairyGUI;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework.Raycasting;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class Pet : MonoBehaviour
{
    // 射线检测标签枚举
    public enum RayHitTag
    {
        Head, Arm, Chest, Cloth, Leg
    }

    [System.Serializable]
    public struct AnimData
    {
        public RayHitTag tag;
        public string trigger; 
    }

    private Animator animator;
    private List<CubismRaycastable> rayAbles = new List<CubismRaycastable>();

    private CubismRaycaster rayCaster;
    private CubismModel cubismModel;
    private bool isActive = false; // 是否正在交互
    public static Pet Instance;
    private void Start()
    {
        cubismModel = GetComponent<CubismModel>();
        rayCaster = GetComponent<CubismRaycaster>();
        animator = GetComponent<Animator>();
        if (Instance == null)
            Instance = this;
        if (cubismModel == null)
        {
            Debug.LogError("没找到model");
            return;
        }
        if (rayCaster == null)
        {
            rayCaster = gameObject.AddComponent<CubismRaycaster>();
            Debug.LogWarning("自动添加CubismRaycaster");
        }
        //自动填充
        if (rayAbles.Count == 0)
        {
            rayAbles.AddRange(GetComponentsInChildren<CubismRaycastable>());
        }
        EventManager.Instance.AddEvent<string>(ClientEvent.ON_PET_EMOTION_CHANGE, SetEmotionState);
    }

    private void LateUpdate()
    {
        // 当所有 UI 关闭时，检测鼠标点击，也就是优先点UI
        if (Input.GetMouseButtonDown(0) && !Stage.isTouchOnUI)
        {
            CheckCubismRaycast();
        }
    }

    // 使用 Cubism 射线检测检查鼠标交互
    private void CheckCubismRaycast()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("没相机");
            return;
        }

        // 创建射线
        //射线检测定义
        CubismRaycaster rayCaster = GetComponent<CubismRaycaster>();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        CubismRaycastHit[] hits = new CubismRaycastHit[rayAbles.Count];
        //返回检测到的数量
        int hitCount = rayCaster.Raycast(ray, hits); 

        // 处理检测结果
        for (int i = 0; i < hitCount; i++)
        {
            string hitTag = hits[i].Drawable.tag; // 获取tag
            RayHitTag rayHitTag;

            // 将字符串标签转换为枚举
            if (Enum.TryParse(hitTag, true, out rayHitTag))
            {
                if (!isActive)//不打断前一个交互，再看看在哪写linww
                {
                    AnimationManager.Instance.SetState(new CubismAniState.HappyState(animator));
                }
                break; // 只处理第一个命中的区域
            }
        }
    }
    public void SetEmotionState(string emotion)
    {
        switch (emotion)
        {
            case "happy":
                AnimationManager.Instance.SetState(new CubismAniState.HappyState(animator));
                break;
            case "sad":
                AnimationManager.Instance.SetState(new CubismAniState.SadState(animator));
                break;
            case "shy":
                AnimationManager.Instance.SetState(new CubismAniState.ShyState(animator));
                break;
            case "mad":
                AnimationManager.Instance.SetState(new CubismAniState.MadState(animator));
                break;
            default:
                AnimationManager.Instance.SetState(new CubismAniState.IdleState(animator));
                break;
        }
    }
    private void OnDestroy()
    {
        EventManager.Instance.RemoveEvent<string>(ClientEvent.ON_PET_EMOTION_CHANGE, SetEmotionState);
    }
}