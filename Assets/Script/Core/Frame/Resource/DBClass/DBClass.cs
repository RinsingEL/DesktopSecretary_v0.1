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

        // ����������Ŀ���
        public class Task : tableBase
        {
            public string TaskID { get; set; }               // ����Ψһ��ʶ
            public string Title { get; set; }             // �������
            public string Description { get; set; }       // ��������
            public int Priority { get; set; }             // �������ȼ�
            public int Status { get; set; }               // ����״̬
            public DateTime? DueDate { get; set; }        // �����ֹ����
            public DateTime StartedAt { get; set; }       // ����ʼʱ��
            public DateTime UpdatedAt { get; set; }       // ����������ʱ��

           

            
        }
    }
}

