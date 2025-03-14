using UnityEngine;

namespace Animation
{
    public abstract class AnimationState
    {
        protected Animator animator;

        public AnimationState(Animator anim)
        {
            this.animator = anim;
        }

        public virtual void Enter()
        {
            Debug.Log($"Entering state: {GetType().Name}");
        }

        public virtual void Update()
        {
        }

        public virtual void Exit()
        {
            Debug.Log($"Exiting state: {GetType().Name}");
        }

        public virtual bool ShouldTransNewState(out AnimationState newState)
        {
            newState = null;
            return false;
        }
        //做插值来平滑转化东湖
        protected void SmoothSetFloat(string parameter, float targetValue, float dampTime = 0.5f)
        {
            float currentValue = animator.GetFloat(parameter);
            float newValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime / dampTime);
            animator.SetFloat(parameter, newValue);
        }
    }
}
