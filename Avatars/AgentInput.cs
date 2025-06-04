using EmberAI.Core;

namespace EmberAI.Avatars
{
    using UnityEngine;
    using UnityEngine.AI;

    [RequireComponent(typeof(NavMeshAgent))]
    public class AgentInput : BaseCharacterInput
    {
        NavMeshAgent _agent;

        public override void InitializeDependencies()
        {
            base.InitializeDependencies();

            _agent = this.GetOrAddComponent<NavMeshAgent>();
        }

        
        public override Vector2 ReadMovementInput()
        {
            Vector3 vel = _agent.desiredVelocity;
            return new Vector2(vel.x, vel.z);
        }
        
        public override bool JumpTriggered() => false;
        public override bool IsRunning()     => false;
        public override bool IsCrouching()   => false;
    }

}