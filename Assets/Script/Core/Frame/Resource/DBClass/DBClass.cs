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

           

            
        }
    }
}

