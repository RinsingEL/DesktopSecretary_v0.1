using FairyGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Framework.Utility;
using Com.Module.Schedule;
using Core.Framework.Event;

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
            WatcherPlugin.Instance.SetCurrentTask(param.title);
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
            m_folderBtn.onClick.Set(OnFolderClick);
        }

        private void OnFolderClick(EventContext context)
        {
            if(m_IsFold.selectedIndex == 0)
            {
                m_IsFold.selectedIndex = 1;
                SetXY(Screen.width - m_folderBtn.width * Screen.width / 1920, 200);
            }
            else
            {
                m_IsFold.selectedIndex = 0;
                SetXY(Screen.width - width * Screen.width / 1920, 200);
            }
        }

        private void OnStartClick()
        {
            if (!isRunning)
            {
                StartTimer();
                WatcherPlugin.Instance.EnterFocus();
            }
            else
            {
                PauseTimer();
                WatcherPlugin.Instance.ExitFocus();
            }
        }

        private void OnResetClick()
        {
            ResetTimer();
            WatcherPlugin.Instance.ExitFocus();
        }

        private void OnFinishClick()
        {
            StopTimer();
            WatcherPlugin.Instance.ExitFocus();
            param.Hide();
            EventManager.Instance.Trigger<string>(ClientEvent.ON_FINISH_FOCUS, param.UUID);
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
