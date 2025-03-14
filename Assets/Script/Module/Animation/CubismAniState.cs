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
                SmoothSetFloat("EmoY", 0);// 播放Idle动画
            }
        }
        public class HappyState : AnimationState
        {
            private bool _shouldTransition = false;
            private string _delayCoroutineId; // 记录延时协程的ID

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
                SmoothSetFloat("EmoY", 0); // 播放Idle动画
            }

            public override bool ShouldTransNewState(out AnimationState newState)
            {
                newState = new IdleState(animator);
                return _shouldTransition;
            }

            // 清理协程
            public override void Exit()
            {
                base.Exit();
                if (!string.IsNullOrEmpty(_delayCoroutineId))
                {
                    CoroutineManager.Instance.StopManagedCoroutine(_delayCoroutineId);
                    _delayCoroutineId = null; // 清空ID
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
                SmoothSetFloat("EmoY", 1);// 播放Idle动画
            }
        }
        public class ShyState : AnimationState
        {
            public ShyState(Animator anim) : base(anim) { }
            public override void Enter()
            {
                base.Enter();
                SmoothSetFloat("EmoX", -1);
                SmoothSetFloat("EmoY", 0);// 播放Idle动画
            }
        }
        public class MadState : AnimationState
        {
            public MadState(Animator anim) : base(anim) { }
            public override void Enter()
            {
                base.Enter();
                SmoothSetFloat("EmoX", 0);
                SmoothSetFloat("EmoY", -1);// 播放Idle动画
            }
        }
    }
}


