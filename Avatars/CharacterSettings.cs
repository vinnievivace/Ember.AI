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
        
        [BoxGroup("Gravity"), Tooltip("Tolerance (time in seconds both feet are not grounded) used to determine if grounded")]
        public float IsGroundedTolerance = 0.25f;

        // --- FOOT IK SETTINGS ---
        [BoxGroup("Foot IK"), Tooltip("Enable or disable foot IK.")]
        public bool footIKEnabled = true;

        [BoxGroup("Foot IK"), Tooltip("Maximum distance to raycast downward from each foot.")]
        public float raycastDistance = 0.5f;

        [BoxGroup("Foot IK"), Tooltip("How high above the ground to place the foot.")]
        public float footHeightOffset = 0.02f;

        [BoxGroup("Foot IK"), Tooltip("Overall weight for foot IK.")]
        [Range(0f, 1f)]
        public float ikWeight = 1f;

        [BoxGroup("Foot IK"), Tooltip("Enable pelvis adjustment for uneven ground.")]
        public bool enablePelvisAdjustment = true;

        [BoxGroup("Foot IK"), Tooltip("Maximum vertical pelvis offset when feet are at different heights.")]
        public float pelvisAdjustmentAmount = 0.08f;

        [BoxGroup("Foot IK"), Tooltip("Smoothing speed for pelvis adjustment.")]
        public float pelvisAdjustmentSpeed = 8f;

        public void SetDefaultValues()
        {
            // Movement
            walkSpeed = 2f;
            runSpeed = 6f;
            crouchSpeed = 1.5f;
            rotationSpeed = 720f;
            accelerationSpeed = 15f;
            decelerationTime = 0.5f;
            // Jumping
            canJump = true;
            jumpForce = 10f;
            // Crouch
            canCrouch = true;
            // Animation
            useRootMotion = false;
            idleAnimationSpeed = 1f;
            // Gravity
            gravity = -10f;
            groundStick = 2f;
            IsGroundedTolerance = 0.5f;
            // Foot IK
            footIKEnabled = true;
            raycastDistance = 0.5f;
            footHeightOffset = 0.02f;
            ikWeight = 1f;
            enablePelvisAdjustment = true;
            pelvisAdjustmentAmount = 0.08f;
            pelvisAdjustmentSpeed = 8f;
        }
    }
}