namespace EmberAI.Avatars
{
    using UnityEngine;
    using UnityEngine.AI;

    [RequireComponent(typeof(NavMeshAgent))]
    public class AgentInput : MonoBehaviour, ICharacterInput
    {
        NavMeshAgent _agent;
        void Awake() => _agent = GetComponent<NavMeshAgent>();

        Vector2 ICharacterInput.ReadMovementInput()
        {
            Vector3 vel = _agent.desiredVelocity;
            return new Vector2(vel.x, vel.z);
        }

        bool ICharacterInput.JumpRequested() => false;
        bool ICharacterInput.IsRunning()     => false;
        bool ICharacterInput.IsCrouching()   => false;
    }

}