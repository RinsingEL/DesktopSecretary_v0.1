using FairyGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Framework.Utility;

namespace Com.Module.Watcher
{
    public partial class UI_WatcherWindow : GComponent
    {
        private WatcherWindow.WatcherWindowParam param;
        private float totalTime;  // 总时间（秒）
        private float currentTime; // 当前剩余时间（秒）
        private bool isRunning;   // 是否正在计时
        private string timerCoroutineId; // 计时协程ID

        public void Init(WatcherWindow.WatcherWindowParam param)
        {
            this.param = param;
            UpdateView();
            SetupButtons();
        }

        private void UpdateView()
        {
            if (param == null) return;

            // 初始化标题和描述
            m_taskTitle.text = param.title;
            m_taskDes.text = param.description;
            
            // 计算总时间（秒）
            totalTime = (float)(param.endTime - param.startTime).TotalSeconds;
            currentTime = totalTime;
            
            // 在时钟组件上显示总时长
            TimeSpan totalTimeSpan = TimeSpan.FromSeconds(totalTime);
            string totalTimeText = $"{totalTimeSpan.Hours:D2}:{totalTimeSpan.Minutes:D2}:{totalTimeSpan.Seconds:D2}";
            m_clockTime.text = totalTimeText;
            
            // 更新时钟显示
            UpdateClockDisplay();
        }

        private void SetupButtons()
        {
            m_startBtn.onClick.Add(OnStartClick);
            m_resetBtn.onClick.Add(OnResetClick);
            m_FinishBtn.onClick.Add(OnFinishClick);
        }

        private void OnStartClick()
        {
            if (!isRunning)
            {
                StartTimer();
            }
            else
            {
                PauseTimer();
            }
        }

        private void OnResetClick()
        {
            ResetTimer();
        }

        private void OnFinishClick()
        {
            StopTimer();
            // TODO: 通知外部计时完成
        }

        private void StartTimer()
        {
            if (!isRunning)
            {
                isRunning = true;
                m_startBtn.text = "暂停";
                if (string.IsNullOrEmpty(timerCoroutineId))
                {
                    timerCoroutineId = CoroutineManager.Instance.StartManagedCoroutine(TimerCoroutine());
                }
            }
        }

        private void PauseTimer()
        {
            isRunning = false;
            m_startBtn.text = "继续";
        }

        private void ResetTimer()
        {
            StopTimer();
            currentTime = totalTime;
            UpdateClockDisplay();
        }

        private void StopTimer()
        {
            isRunning = false;
            m_startBtn.text = "开始";
            if (!string.IsNullOrEmpty(timerCoroutineId))
            {
                CoroutineManager.Instance.StopManagedCoroutine(timerCoroutineId);
                timerCoroutineId = null;
            }
        }

        private IEnumerator TimerCoroutine()
        {
            while (currentTime > 0)
            {
                if (isRunning)
                {
                    currentTime -= Time.deltaTime;
                    UpdateClockDisplay();
                }
                yield return null;
            }
            
            StopTimer();
            // TODO: 通知外部计时完成
        }

        private void UpdateClockDisplay()
        {
            // 更新时钟填充
            float fillAmount = currentTime / totalTime;
            m_clock.fillAmount = fillAmount;

            // 更新时间显示
            TimeSpan timeSpan = TimeSpan.FromSeconds(currentTime);
            string timeText = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            m_clockTime.text = timeText;
        }
    }
}
