// PluginBase.cs
using System;
using UnityEngine.EventSystems;

namespace Core.Framework.Plugin
{
    /// <summary>
    /// Plugin ���࣬���̳� MonoBehaviour���ṩ����ģʽ���¼��������������ڹ���
    /// </summary>
    public abstract class PluginBase
    {
        // �Ƿ��ѳ�ʼ��
        private bool _isInitialized = false;

        /// <summary>
        /// ע�������ֶ����û�ͨ������������������
        /// </summary>
        public void Register()
        {
            if (_isInitialized) return;
            OnRegister();
            _isInitialized = true;
        }

        /// <summary>
        /// ж�ز��
        /// </summary>
        public void Uninstall()
        {
            if (!_isInitialized) return;
            OnUninstall();
            _isInitialized = false;
        }

        //--------------------------------------------------
        // �鷽��������ʵ�֣�
        //--------------------------------------------------
        /// <summary>
        /// ע���¼�����ʼ�����ݵ�
        /// </summary>
        protected abstract void OnRegister();

        /// <summary>
        /// �����¼����ͷ���Դ��
        /// </summary>
        protected abstract void OnUninstall();
    }
}