using Core;
using EmberAI.Attributes;
using EmberAI.Core;
using UnityEngine;

namespace EmberAI.Avatars
{
    [CreateAssetMenu(fileName = "CharacterSettings", menuName = EmberAISystem.MenuPath + "/Settings/CharacterSettings", order = 1)]
    public class CharacterSettings : BaseData
    {
        [BoxGroup("Movement")]
        public EmberBehaviour.UpdateMode updateMode = EmberBehaviour.UpdateMode.FixedUpdate;
        
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
        
        [BoxGroup("Crouch")]
        public bool  canCrouch = true;
        
        [BoxGroup("Animation")]
        public RuntimeAnimatorController animatorController;
        
        [BoxGroup("Animation")]
        public bool useRootMotion = true;
        
        [BoxGroup("Animation"), Tooltip("Idle animation playback speed.")]     
        public float idleAnimationSpeed = 1f;
        
        [BoxGroup("Gravity")]
        public float gravity = -9.81f;
        
        [BoxGroup("Gravity"), Tooltip("Downward velocity when grounded to keep snapped.")] 
        public float groundStick = 2f;
        
        [BoxGroup("Foot IK")]
        [Tooltip("How high above the root (body) the foot can be and still be considered grounded. Increase if idle feet float above ground. Typical: 0.02–0.05.")]
        public float groundedTolerance = 0.03f;

        [BoxGroup("Foot IK")]
        [Tooltip("Enable look-ahead for steps/bumps. If true, the system anticipates upcoming changes in ground height for more natural foot placement.")]
        public bool enableForwardStepDetection = true;

        [BoxGroup("Foot IK")]
        [Tooltip("How far ahead (in meters) to look for steps/bumps when forward step detection is enabled. Increase for bigger steps. Typical: 0.2–0.4.")]
        public float forwardStepDetectionRange = 0.25f;

        [BoxGroup("Foot IK")]
        [Tooltip("How far down (in meters) to search for ground below each foot. Increase for big drops/steps. Typical: 0.4–0.6.")]
        public float footRaycastDistance = 0.5f;

        [BoxGroup("Foot IK")]
        [Tooltip("Fine-tune body (pelvis) height above ground after foot IK. Use small values (e.g., -0.05 to 0.05) to correct floating/sinking. Usually 0.")]
        public float bodyHeightOffset = 0f;

        [BoxGroup("Foot IK")]
        [Tooltip("Smoothing for all transitions (foot placement, body offset, etc). 0 = instant, 1 = very slow. Typical: 0.3–0.7.")]
        public float ikSmoothing = 0.5f;
    }
}