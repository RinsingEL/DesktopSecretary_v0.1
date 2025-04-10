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
using UnityEngine;

namespace Core.Framework.Pet
{
    public class Pet : MonoBehaviour
    {
        // 射线检测标签枚举
        public enum RayHitTag
        {
            Head, Arm, Chest, Cloth, Leg , Body
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
        public PetAttributes attributes;

        // 拖动相关
        private bool isDragging = false;
        private Vector3 dragOffset;

        // 摸头好感度限制（使用 PlayerPrefs 持久化）
        private const string HEAD_PAT_COUNT_KEY = "HeadPatCountToday";
        private const string LAST_PAT_DATE_KEY = "LastPatDate";
        private int headPatCountToday = 0;
        private DateTime lastPatDate = DateTime.MinValue;

        private void Start()
        {
            cubismModel = GetComponent<CubismModel>();
            rayCaster = GetComponent<CubismRaycaster>();
            animator = GetComponent<Animator>();
            attributes = GetComponent<PetAttributes>();

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
            if (attributes == null)
            {
                attributes = gameObject.AddComponent<PetAttributes>();
                Debug.LogWarning("自动添加PetAttributes");
            }

            if (rayAbles.Count == 0)
            {
                rayAbles.AddRange(GetComponentsInChildren<CubismRaycastable>());
            }
            EventManager.Instance.AddEvent<string>(ClientEvent.ON_PET_EMOTION_CHANGE, SetEmotionState);

            // 加载持久化数据
            LoadHeadPatData();
        }

        private void Update()
        {
            // 检查鼠标拖动
            if (Input.GetMouseButtonDown(0) && !Stage.isTouchOnUI)
            {
                StartDragging();
            }
            if (Input.GetMouseButton(0) && isDragging)
            {
                DragPet();
            }
            if (Input.GetMouseButtonUp(0))
            {
                StopDragging();
            }
        }

        private void LateUpdate()
        {
            // 当所有 UI 关闭时，检测鼠标点击（用于摸头）
            if (Input.GetMouseButtonDown(0) && !Stage.isTouchOnUI)
            {
                CheckCubismRaycast();
            }
        }

        // 开始拖动
        private void StartDragging()
        {
            CubismRaycastHit[] hits = PerformRaycast();
            if (hits.Length > 0)
            {
                RayHitTag hitTag;
                if (Enum.TryParse(hits[0].Drawable.tag, true, out hitTag) && hitTag == RayHitTag.Body)
                {
                    isDragging = true;
                    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mouseWorldPos.z = transform.position.z; // 保持 Z 轴不变
                    dragOffset = transform.position - mouseWorldPos;
                }
            }
        }

        // 拖动桌宠
        private void DragPet()
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = transform.position.z; // 保持 Z 轴不变
            transform.position = mouseWorldPos + dragOffset;
        }

        // 停止拖动
        private void StopDragging()
        {
            isDragging = false;
        }

        // 使用 Cubism 射线检测检查鼠标交互
        private CubismRaycastHit[] PerformRaycast()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("没相机");
                return new CubismRaycastHit[0];
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            CubismRaycastHit[] hits = new CubismRaycastHit[rayAbles.Count];
            int hitCount = rayCaster.Raycast(ray, hits);
            CubismRaycastHit[] result = new CubismRaycastHit[hitCount];
            Array.Copy(hits, result, hitCount);
            return result;
        }

        private void CheckCubismRaycast()
        {
            CubismRaycastHit[] hits = PerformRaycast();
            for (int i = 0; i < hits.Length; i++)
            {
                string hitTag = hits[i].Drawable.tag;
                RayHitTag rayHitTag;

                if (Enum.TryParse(hitTag, true, out rayHitTag))
                {
                    if (!isActive)
                    {
                        if (rayHitTag == RayHitTag.Head)
                        {
                            TryPatHead();
                        }
                        else
                        {
                            AnimationManager.Instance.SetState(new CubismAniState.HappyState(animator));
                        }
                    }
                    break; // 只处理第一个命中的区域
                }
            }
        }

        // 加载摸头数据
        private void LoadHeadPatData()
        {
            // 读取上次摸头日期
            string lastDateStr = PlayerPrefs.GetString(LAST_PAT_DATE_KEY, DateTime.MinValue.ToString("yyyy-MM-dd"));
            if (DateTime.TryParseExact(lastDateStr, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime savedDate))
            {
                lastPatDate = savedDate;
            }
            else
            {
                lastPatDate = DateTime.MinValue;
            }

            // 检查是否为新的一天
            DateTime today = DateTime.Today;
            if (lastPatDate != today)
            {
                headPatCountToday = 0;
                lastPatDate = today;
                SaveHeadPatData(); // 更新日期
            }
            else
            {
                headPatCountToday = PlayerPrefs.GetInt(HEAD_PAT_COUNT_KEY, 0); // 读取当天次数
            }
        }

        // 保存摸头数据
        private void SaveHeadPatData()
        {
            PlayerPrefs.SetInt(HEAD_PAT_COUNT_KEY, headPatCountToday);
            PlayerPrefs.SetString(LAST_PAT_DATE_KEY, lastPatDate.ToString("yyyy-MM-dd"));
            PlayerPrefs.Save();
        }

        // 摸头增加好感度
        private void TryPatHead()
        {
            // 检查是否为新的一天
            DateTime today = DateTime.Today;
            if (lastPatDate != today)
            {
                headPatCountToday = 0;
                lastPatDate = today;
            }

            // 检查每日限制
            if (headPatCountToday < 3)
            {
                attributes.IncreaseFavorability(1f);
                headPatCountToday++;
                SaveHeadPatData(); // 保存更新后的数据
                AnimationManager.Instance.SetState(new CubismAniState.ShyState(animator));
                Debug.Log($"摸头成功，好感度 +1，当前好感度: {attributes.GetFavorability()}，今日剩余次数: {3 - headPatCountToday}");
            }
            else
            {
                Debug.Log("今天摸头次数已达上限！");
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
            SaveHeadPatData(); // 退出时保存数据
        }
    }
}