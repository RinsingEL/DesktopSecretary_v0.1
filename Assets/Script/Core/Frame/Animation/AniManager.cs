using UnityEngine;
namespace Animation
{
    public class AnimationManager : MonoBehaviour
    {
        #region 单例模式实现
        private static AnimationManager instance;

        public static AnimationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AnimationManager>();
                    if (instance == null)
                    {
                        GameObject singletonObject = new GameObject("AnimationManager");
                        instance = singletonObject.AddComponent<AnimationManager>();
                    }
                }
                return instance;
            }
        }

        void Awake()
        {
            // 确保只有一个实例
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator?");
            }

            SetState(new CubismAniState.IdleState(animator));
        }
        #endregion

        private Animator animator;
        private AnimationState currentState; // 当前动画状态
        void LateUpdate()
        {
            if (currentState != null)
            {
                currentState.Update();

                // 检查是否需要切换状态
                if (currentState.ShouldTransNewState(out AnimationState nextState))
                {
                    SwitchState(nextState);
                }
            }
        }

        // 设置新的状态
        public void SetState(AnimationState newState)
        {
            if (currentState != null)
            {
                currentState.Exit(); // 退出当前状态
            }

            currentState = newState;

            if (currentState != null)
            {
                currentState.Enter(); // 进入新状态
            }
        }

        // 切换状态
        private void SwitchState(AnimationState nextState)
        {
            if (nextState != null)
            {
                SetState(nextState);
            }
        }

        public void TriggerStateChange(string trigger)
        {
            animator.SetTrigger(trigger);
        }

        public Animator GetAnimator()
        {
            return animator;
        }
    }
}
