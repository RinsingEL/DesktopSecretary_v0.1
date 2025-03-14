using UnityEngine;
namespace Animation
{
    public class AnimationManager : MonoBehaviour
    {
        #region ����ģʽʵ��
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
            // ȷ��ֻ��һ��ʵ��
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
        private AnimationState currentState; // ��ǰ����״̬
        void LateUpdate()
        {
            if (currentState != null)
            {
                currentState.Update();

                // ����Ƿ���Ҫ�л�״̬
                if (currentState.ShouldTransNewState(out AnimationState nextState))
                {
                    SwitchState(nextState);
                }
            }
        }

        // �����µ�״̬
        public void SetState(AnimationState newState)
        {
            if (currentState != null)
            {
                currentState.Exit(); // �˳���ǰ״̬
            }

            currentState = newState;

            if (currentState != null)
            {
                currentState.Enter(); // ������״̬
            }
        }

        // �л�״̬
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
