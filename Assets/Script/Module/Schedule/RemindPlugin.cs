using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Framework.Utility;
using Core.Framework.Event;
using Core.Framework.Network;
using Com.Module.Chat;
using Com.Module.Schedule;
using Core.Framework.Resource;
using Module.chat;
using Core.Framework.Plugin;

public class RemindPlugin : PluginBase
{
    private string checkCoroutineId;
    private CalendarViewModel _viewModel;
    private HashSet<string> _processedTasks = new HashSet<string>();

    protected override void OnRegister()
    {
        _viewModel = CalendarViewModel.Instance;
        checkCoroutineId = CoroutineManager.Instance.StartManagedCoroutine(CheckTaskReminder());
        LoadProcessedTasks();
    }

    protected override void OnUpdate()
    {
    }

    protected override void OnUninstall()
    {
        if (!string.IsNullOrEmpty(checkCoroutineId))
        {
            CoroutineManager.Instance.StopManagedCoroutine(checkCoroutineId);
        }
        _processedTasks.Clear();
    }

    private IEnumerator CheckTaskReminder()
    {
        while (true)
        {
            yield return new WaitForSeconds(30); // 每30秒检查一次
            
            var allTasks = GetAllValidTasks();
            foreach (var task in allTasks)
            {
                if (ShouldRemind(task) && !_processedTasks.Contains(task.TaskID))
                {
                    SendReminderToGPT(task);
                    MarkAsProcessed(task);
                }
            }
        }
    }

    private List<DBClass.Task> GetAllValidTasks()
    {
        return _viewModel.GetAllTasks().FindAll(t => 
            t.Status == 0 || t.Status == 1); // 只处理未完成和进行中的任务
    }

    private bool ShouldRemind(DBClass.Task task)
    {
        return task.DueDate.HasValue && 
               DateTime.Now >= task.DueDate.Value.AddMinutes(-15) && 
               DateTime.Now <= task.DueDate.Value;
    }

    private void SendReminderToGPT(DBClass.Task task)
    {
        string prompt = $"现在到{task.Title}的时间了，你需要提醒用户（当前时间：{DateTime.Now:HH:mm}）\n" +
                       $"任务标题：{task.Title}\n" +
                       $"任务描述：{task.Description}\n" +
                       $"截止时间：{task.DueDate:HH:mm}";

        ChatPlugin.Instance.OnSendFunctionRequest(prompt);
    }

    private void MarkAsProcessed(DBClass.Task task)
    {
        _processedTasks.Add(task.TaskID);
        SaveProcessedTasks();
        
        // 更新任务状态为进行中（如果当前是未完成状态）
        if (task.Status == 0)
        {
            task.Status = 1;
            _viewModel.UpdateTask(task);
        }
    }

    private void LoadProcessedTasks()
    {
        string savedData = PlayerPrefs.GetString("ProcessedTasks", "");
        if (!string.IsNullOrEmpty(savedData))
        {
            _processedTasks = new HashSet<string>(savedData.Split(','));
        }
    }

    private void SaveProcessedTasks()
    {
        PlayerPrefs.SetString("ProcessedTasks", string.Join(",", _processedTasks));
        PlayerPrefs.Save();
    }
}
