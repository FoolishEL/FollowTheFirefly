using System;
using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Interfaces;
using UnityEngine;

namespace Demos.Shared.Behaviours
{
    public class AgentMoveBehaviour : MonoBehaviour
    {
        private AgentBehaviour agent;
        private ITarget currentTarget;
        private bool shouldMove;

        private void Awake()
        {
            agent = GetComponent<AgentBehaviour>();
        }

        private void OnEnable()
        {
            agent.Events.OnTargetInRange += OnTargetInRange;
            agent.Events.OnTargetChanged += OnTargetChanged;
            agent.Events.OnTargetOutOfRange += OnTargetOutOfRange;
        }

        private void OnDisable()
        {
            agent.Events.OnTargetInRange -= OnTargetInRange;
            agent.Events.OnTargetChanged -= OnTargetChanged;
            agent.Events.OnTargetOutOfRange -= OnTargetOutOfRange;
        }

        private void OnTargetInRange(ITarget target)
        {
            shouldMove = false;
        }

        private void OnTargetChanged(ITarget target, bool inRange)
        {
            currentTarget = target;
            shouldMove = !inRange;
        }

        private void OnTargetOutOfRange(ITarget target)
        {
            shouldMove = true;
        }

        public void Update()
        {
            if (!shouldMove)
                return;
            
            if (currentTarget == null)
                return;
            
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(currentTarget.Position.x, transform.position.y, currentTarget.Position.z), Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            if (currentTarget == null)
                return;
            
            Gizmos.DrawLine(transform.position, currentTarget.Position);
        }
    }
}