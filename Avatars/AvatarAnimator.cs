using System.Collections.Generic;
using EmberAI.Attributes;
using EmberAI.Core;
using UnityEngine;

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
        
        [BoxGroup("Components"), SerializeField] 
        private Animator animator;
        
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

        #endregion

        #region General ................................................................................................

        public void InitializeAnimator(RuntimeAnimatorController controller, bool useRootMotion)
        {
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = useRootMotion;
        }

        public void SetAnimatorAvatar(Avatar avatar, AvatarConfig config)
        {
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
        
        public void UpdateAnimatorParams(bool isMoving, float speed, bool jumped, bool isGrounded, float inputMag, float verticalVelocity)
        {
            if (animator == null) return;

            animator.SetFloat(SpeedID, isMoving ? speed : 0f);
            animator.SetBool(JumpID, jumped);
            animator.SetBool(GroundedID, isGrounded);

            bool isFalling = !isGrounded && verticalVelocity < 0f;
            animator.SetBool(FreeFallID, isFalling);

            float motionSpeed = isMoving ? inputMag : 1;
            
            animator.SetFloat(MotionSpeedID, motionSpeed);
        }
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

#endregion
    }
}