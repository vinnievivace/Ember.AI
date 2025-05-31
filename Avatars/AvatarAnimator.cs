using System.Collections.Generic;
using EmberAI.Attributes;
using EmberAI.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace EmberAI.Avatars
{
    public class AvatarAnimator : EmberBehaviour
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        public enum AnimatorState { Idle, Walk, Run, JumpStart, JumpLand, JumpLandWalk, JumpLandRun, InAir }
        
        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        private static readonly int SpeedID       = Animator.StringToHash("Speed");
        private static readonly int JumpID        = Animator.StringToHash("Jump");
        private static readonly int GroundedID    = Animator.StringToHash("Grounded");
        private static readonly int FreeFallID    = Animator.StringToHash("FreeFall");
        private static readonly int MotionSpeedID = Animator.StringToHash("MotionSpeed");
        
        [BoxGroup("Settings"), ReadOnly, SerializeField]
        private CharacterSettings CharacterSettings;
        
        [BoxGroup("Settings"), ReadOnly, SerializeField]
        private AvatarConfig avatarConfig;
        
        [BoxGroup("Components"), SerializeField] 
        private Animator animator;
        
        [BoxGroup("State")] 
        public bool active = true;
        
        [BoxGroup("Foot IK"), Tooltip("Maximum distance to raycast downward from each foot.")]
        public float raycastDistance = 1.5f;
        
        [BoxGroup("Foot IK"),Tooltip("How high above the ground to place the foot.")]
        public float footHeightOffset = 0.1f;

        [BoxGroup("Foot IK"),Tooltip("Overall weight for foot IK.")]
        [Range(0f, 1f)]
        public float ikWeight = 1f;
        
        [BoxGroup("Foot IK"), Tooltip("Weight for knee hint positioning.")]
        [Range(0f, 1f)]
        public float kneeHintWeight = 1f;

        [BoxGroup("Foot IK"), Tooltip("Local offset forward from the knee for hinting bend direction.")]
        public float kneeHintForward = 0.3f;
        
        [BoxGroup("Foot IK"), Tooltip("Local offset outward from the thigh for hinting bend direction.")]
        public float kneeHintOutward = 0.1f;
        
        
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        #endregion

        #region Inspector ..............................................................................................

        #endregion

        #region Initialization .........................................................................................

        public override void InitializeDependencies()
        {
            base.InitializeDependencies();

            description = "Humanoid Avatar Animator.";

            animator = this.GetOrAddComponent<Animator>();
        }

        #endregion

        #region MonoBehaviours .........................................................................................

        protected override void OnUpdate()
        {
            base.OnUpdate();
            
            animator.enabled = active;
        }

        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
            
            if(avatarConfig == null || avatarConfig.shoulderXOffset == 0) return;
            
            AvatarBuilder.ApplyShoulderOffset(transform, avatarConfig);
        }

        #endregion

        #region General ................................................................................................

        public void InitializeAnimator(CharacterSettings settings)
        {
            CharacterSettings = settings;
            
            animator.runtimeAnimatorController = settings.animatorController;
            animator.applyRootMotion = settings.useRootMotion;
        }

        public void InitializeAvatar(Avatar avatar, AvatarConfig config)
        {
            avatarConfig = config;
            animator.avatar = avatar;
            
            AnimatorOverrideController overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            
            // Copy current overrides into a list
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);
            
            ApplyOverrideAnimation(config.idle, AnimatorState.Idle, overrides);
            ApplyOverrideAnimation(config.inAir, AnimatorState.InAir, overrides);
            ApplyOverrideAnimation(config.jumpStart, AnimatorState.JumpStart, overrides);
            ApplyOverrideAnimation(config.jumpLand, AnimatorState.JumpLand, overrides);
            ApplyOverrideAnimation(config.jumpLandWalk, AnimatorState.JumpLandWalk, overrides);
            ApplyOverrideAnimation(config.jumpLandRun, AnimatorState.JumpLandRun, overrides);
            ApplyOverrideAnimation(config.walk, AnimatorState.Walk, overrides);
            ApplyOverrideAnimation(config.run, AnimatorState.Run, overrides);

            overrideController.ApplyOverrides(overrides);
            
            animator.runtimeAnimatorController = overrideController;
            animator.Rebind();
            
        }
        
        private void ApplyOverrideAnimation(AnimationClip clip, AnimatorState state, List<KeyValuePair<AnimationClip, AnimationClip>> overrides)
        {
            if (clip == null) return;
            
            var overrideState = overrides.Find(i => i.Key.name == state.ToString());
            int idleIndex = overrides.IndexOf(overrideState);
            
            overrides[idleIndex] = new KeyValuePair<AnimationClip, AnimationClip>(overrideState.Key, clip);
            
        }
        
        public void UpdateAnimatorParams(float currentSpeed, float maxSpeed, bool jumped, bool isGrounded, float verticalVelocity)
        {
            if (animator == null) return;

            // 1) SPEED PARAMETER: zero when idle, otherwise actual speed
            bool isMoving = currentSpeed > 0.001f;
            
            animator.SetFloat(SpeedID, isMoving ? currentSpeed : 0f);

            // 2) JUMP / GROUNDED / FREEFALL
            bool isFalling = !isGrounded && verticalVelocity < 0f;
            
            animator.SetBool(JumpID, jumped);
            animator.SetBool(GroundedID, isGrounded);
            animator.SetBool(FreeFallID, isFalling);

            // 3) MOTION SPEED: If moving, use (currentSpeed / maxSpeed) so the walk/run blend matches actual speed.
            // If fully stopped, set to 1 so Idle plays at normal rate.
            float motionSpeed = isMoving && maxSpeed > 0f ? Mathf.Clamp01(currentSpeed / maxSpeed) : 1f;
            
            animator.SetFloat(MotionSpeedID, motionSpeed);
        }

        
        #endregion
        
        #region Foot IK ................................................................................................

        private void OnAnimatorIK(int layerIndex)
        {
            if(!active) return;

            ApplyFootIK(AvatarIKGoal.LeftFoot, AvatarIKHint.LeftKnee);
            ApplyFootIK(AvatarIKGoal.RightFoot, AvatarIKHint.RightKnee);
        }

        private void ApplyFootIK(AvatarIKGoal foot, AvatarIKHint kneeHint)
        {
            animator.SetIKPositionWeight(foot, ikWeight);
            animator.SetIKRotationWeight(foot, ikWeight);
            animator.SetIKHintPositionWeight(kneeHint, kneeHintWeight);
            
            Vector3 footPos = animator.GetIKPosition(foot);
            Quaternion footRot = animator.GetIKRotation(foot);

            if (Physics.Raycast(footPos + Vector3.up * raycastDistance, Vector3.down, out RaycastHit hit, raycastDistance * 2f, CharacterSettings.groundLayer))
            {
                // Target foot position & rotation aligned to ground normal
                Vector3 targetPos = hit.point + Vector3.up * footHeightOffset;
                Quaternion targetRot = Quaternion.LookRotation(
                    Vector3.ProjectOnPlane(transform.forward, hit.normal),
                    hit.normal
                );

                // Apply IK position and rotation
                animator.SetIKPosition(foot, Vector3.Lerp(footPos, targetPos, ikWeight));
                animator.SetIKRotation(foot, Quaternion.Slerp(footRot, targetRot, ikWeight));

                // Compute knee hint position based on thigh orientation
                Transform thigh = animator.GetBoneTransform(
                    foot == AvatarIKGoal.LeftFoot ? HumanBodyBones.LeftUpperLeg : HumanBodyBones.RightUpperLeg);

                Vector3 hintDirection = (transform.forward * kneeHintForward) +
                                         (transform.right   * (foot == AvatarIKGoal.LeftFoot ? -kneeHintOutward : kneeHintOutward));

                Vector3 hintPos = thigh.position + hintDirection;
                animator.SetIKHintPosition(kneeHint, hintPos);
            }
            else
            {
                // No ground hit: restore original
                animator.SetIKPosition(foot, footPos);
                animator.SetIKRotation(foot, footRot);
            }
        }
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

        #endregion
    }
}