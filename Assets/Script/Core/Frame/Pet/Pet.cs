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
    // ���߼���ǩö��
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
    private bool isActive = false; // �Ƿ����ڽ���
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
            Debug.LogError("û�ҵ�model");
            return;
        }
        if (rayCaster == null)
        {
            rayCaster = gameObject.AddComponent<CubismRaycaster>();
            Debug.LogWarning("�Զ����CubismRaycaster");
        }
        //�Զ����
        if (rayAbles.Count == 0)
        {
            rayAbles.AddRange(GetComponentsInChildren<CubismRaycastable>());
        }
        EventManager.Instance.AddEvent<string>(ClientEvent.ON_PET_EMOTION_CHANGE, SetEmotionState);
    }

    private void LateUpdate()
    {
        // ������ UI �ر�ʱ������������Ҳ�������ȵ�UI
        if (Input.GetMouseButtonDown(0) && !Stage.isTouchOnUI)
        {
            CheckCubismRaycast();
        }
    }

    // ʹ�� Cubism ���߼������꽻��
    private void CheckCubismRaycast()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("û���");
            return;
        }

        // ��������
        //���߼�ⶨ��
        CubismRaycaster rayCaster = GetComponent<CubismRaycaster>();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        CubismRaycastHit[] hits = new CubismRaycastHit[rayAbles.Count];
        //���ؼ�⵽������
        int hitCount = rayCaster.Raycast(ray, hits); 

        // ��������
        for (int i = 0; i < hitCount; i++)
        {
            string hitTag = hits[i].Drawable.tag; // ��ȡtag
            RayHitTag rayHitTag;

            // ���ַ�����ǩת��Ϊö��
            if (Enum.TryParse(hitTag, true, out rayHitTag))
            {
                if (!isActive)//�����ǰһ���������ٿ�������дlinww
                {
                    AnimationManager.Instance.SetState(new CubismAniState.HappyState(animator));
                }
                break; // ֻ�����һ�����е�����
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