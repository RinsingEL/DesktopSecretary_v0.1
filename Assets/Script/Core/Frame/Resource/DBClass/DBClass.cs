using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Framework.Resource
{
    public class DBClass
    {
        public class tableBase
        {

        }

        // 用于日历表的开发
        public class Task : tableBase
        {
            public string TaskID { get; set; }               // 任务唯一标识
            public string Title { get; set; }             // 任务标题
            public string Description { get; set; }       // 任务描述
            public int Priority { get; set; }             // 任务优先级
            public int Status { get; set; }               // 任务状态
            public DateTime? DueDate { get; set; }        // 任务截止日期
            public DateTime StartedAt { get; set; }       // 任务开始时间
            public DateTime UpdatedAt { get; set; }       // 任务最后更新时间

            public Task()
            {
                Priority = 2;  // 默认优先级
                Status = 0;    // 默认状态
                StartedAt = DateTime.Now;
                UpdatedAt = DateTime.Now;
            }
        }

        public class TaskEvaluation : tableBase
        {
            public string EvaluationID { get; set; }     // 评估唯一标识
            public string Tasked { get; set; }           // 关联的任务ID
            public string EvaluationContent { get; set; }// 评估内容
            public int Duration { get; set; }            // 任务持续时间（秒）
            public DateTime EvaluationTime { get; set; } // 评估时间

            public TaskEvaluation()
            {
                EvaluationTime = DateTime.Now;
            }
        }
    }
}

