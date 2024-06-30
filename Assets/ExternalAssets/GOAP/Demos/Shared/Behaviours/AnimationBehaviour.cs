using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Enums;
using UnityEngine;

namespace Demos.Shared.Behaviours
{
    public class AnimationBehaviour : MonoBehaviour
    {
        private Animator animator;
        private AgentBehaviour agent;
        private static readonly int Walking = Animator.StringToHash("Walking");

        private bool isWalking;
        private bool isMovingLeft;

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            agent = GetComponent<AgentBehaviour>();
            
            // Random y offset to prevent clipping
            animator.transform.localPosition = new Vector3(0, Random.Range(-0.1f, 0.1f), 0);
        }

        private void Update()
        {
            UpdateAnimation();
            UpdateScale();
        }

        private void UpdateAnimation()
        {
            var isWalking = agent.State == AgentState.MovingToTarget;

            if (this.isWalking == isWalking)
                return;

            this.isWalking = isWalking;
            
            animator.SetBool(Walking, isWalking);
        }

        private void UpdateScale()
        {
            if (!isWalking)
                return;
            
            var isMovingLeft = IsMovingLeft();

            if (this.isMovingLeft == isMovingLeft)
                return;

            this.isMovingLeft = isMovingLeft;
            
            animator.transform.localScale = new Vector3(isMovingLeft ? -1 : 1, 1, 1);
        }

        private bool IsMovingLeft()
        {
            var target = agent.CurrentActionData.Target.Position;
            
            return transform.position.x > target.x;
        }
    }
}