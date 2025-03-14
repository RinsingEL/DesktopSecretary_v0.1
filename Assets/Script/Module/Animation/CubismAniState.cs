using Core.Framework.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animation
{
    public class CubismAniState
    {
        public class IdleState : AnimationState
        {
            public IdleState(Animator anim) : base(anim){}
            public override void Enter()
            {
                base.Enter();
            }
            public override void Update()
            {
                base.Update();
                SmoothSetFloat("EmoX", 0);
                SmoothSetFloat("EmoY", 0);// ����Idle����
            }
        }
        public class HappyState : AnimationState
        {
            private bool _shouldTransition = false;
            private string _delayCoroutineId; // ��¼��ʱЭ�̵�ID

            public HappyState(Animator anim) : base(anim) { }

            public override void Enter()
            {
                base.Enter();

                _shouldTransition = false;
                _delayCoroutineId = CoroutineManager.Instance.Delay(3f, () => _shouldTransition = true);
            }
            public override void Update()
            {
                base.Update();
                SmoothSetFloat("EmoX", 1);
                SmoothSetFloat("EmoY", 0); // ����Idle����
            }

            public override bool ShouldTransNewState(out AnimationState newState)
            {
                newState = new IdleState(animator);
                return _shouldTransition;
            }

            // ����Э��
            public override void Exit()
            {
                base.Exit();
                if (!string.IsNullOrEmpty(_delayCoroutineId))
                {
                    CoroutineManager.Instance.StopManagedCoroutine(_delayCoroutineId);
                    _delayCoroutineId = null; // ���ID
                }
            }
        }
        public class SadState : AnimationState
        {
            public SadState(Animator anim) : base(anim) { }
            public override void Enter()
            {
                base.Enter();
                SmoothSetFloat("EmoX", 0);
                SmoothSetFloat("EmoY", 1);// ����Idle����
            }
        }
        public class ShyState : AnimationState
        {
            public ShyState(Animator anim) : base(anim) { }
            public override void Enter()
            {
                base.Enter();
                SmoothSetFloat("EmoX", -1);
                SmoothSetFloat("EmoY", 0);// ����Idle����
            }
        }
        public class MadState : AnimationState
        {
            public MadState(Animator anim) : base(anim) { }
            public override void Enter()
            {
                base.Enter();
                SmoothSetFloat("EmoX", 0);
                SmoothSetFloat("EmoY", -1);// ����Idle����
            }
        }
    }
}


