using EmberAI.Attributes;
using EmberAI.Core;
using UnityEngine;

namespace EmberAI.Avatars
{
    [CreateAssetMenu(menuName = "Characters/Character Settings")]
    public class CharacterSettings : BaseData
    {
        [BoxGroup("Movement")]
        public EmberBehaviour.UpdateMode updateType = EmberBehaviour.UpdateMode.FixedUpdate;
        
        [BoxGroup("Movement")]
        public float walkSpeed = 3f;
        
        [BoxGroup("Movement")]
        public float runSpeed = 6f;
        
        [BoxGroup("Movement")]
        public float crouchSpeed = 1.5f;
        
        [BoxGroup("Movement")]
        [Tooltip("Degrees per second to turn toward movement direction")]
        public float rotationSpeed = 720;
        
        [BoxGroup("Movement"), Tooltip("Units/sec to ramp up to target speed.")]
        public float accelerationSpeed = 15f;

        [BoxGroup("Movement"), Tooltip("Seconds to ramp from stop‐speed → 0.")]
        public float decelerationTime = 0.5f;

        [BoxGroup("Movement")]
        [InspectorText, SerializeField]
        private string keyboardMappingInfo = "NOTE: Keyboard Mappings assigned on " + nameof(KeyboardMouseInput) + " Component";
        
        [BoxGroup("Jumping")]
        public bool  canJump = true;
        
        [BoxGroup("Jumping")]
        public float jumpForce = 7f;
        
        [BoxGroup("Jumping")]
        public float gravity = -9.81f;

        [BoxGroup("Crouch")]
        public bool  canCrouch = true;
        
        [BoxGroup("Crouch")]
        public float crouchHeight = 1f;
        
        [BoxGroup("Animation")]
        public RuntimeAnimatorController animatorController;
        
        [BoxGroup("Animation")]
        public bool useRootMotion = true;
        
        [BoxGroup("Animation"), Tooltip("Idle animation playback speed.")]     
        public float idleAnimationSpeed = 1f;
        
        [BoxGroup("Ground"), Tooltip("Layers considered as ground.")]
        public LayerMask groundLayer;
        
        [BoxGroup("Ground"), Tooltip("Downward velocity when grounded to keep snapped.")] 
        public float groundStick = 2f;
    }
}