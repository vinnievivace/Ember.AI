using System.Collections.Generic;
using EmberAI.Attributes;
using EmberAI.Core;
using EmberAI.Envrionment;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EmberAI.Avatars
{
    public class AvatarAnimator : EmberBehaviour
    {
        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        public enum AnimatorState { Idle, Walk, Run, JumpStart, JumpLand, JumpLandWalk, JumpLandRun, Crouch, InAir }

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        private static readonly int SpeedID       = Animator.StringToHash("Speed");
        private static readonly int JumpID        = Animator.StringToHash("Jump");
        private static readonly int DanceID       = Animator.StringToHash("Dance");
        private static readonly int CrouchID      = Animator.StringToHash("Crouch");
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

        [BoxGroup("Foot IK"), Tooltip("How high above the ground to place the foot.")]
        public float footHeightOffset = 0.1f;

        [BoxGroup("Foot IK"), Tooltip("Overall weight for foot IK.")]
        [Range(0f, 1f)]
        public float ikWeight = 1f;

        [BoxGroup("Foot IK"), Tooltip("Weight for knee hint positioning.")]
        [Range(0f, 1f)]
        public float kneeHintWeight = 1f;

        [BoxGroup("Foot IK"), Tooltip("Local offset forward from the knee for hinting bend direction.")]
        public float kneeHintForward = 0.3f;

        [BoxGroup("Foot IK"), Tooltip("Local offset outward from the thigh for hinting bend direction.")]
        public float kneeHintOutward = 0.1f;

        [BoxGroup("Debug"), SerializeField]
        private bool drawDebugGizmos = false;

        // Tracks whether we’ve already hidden child renderers (so we only do it once)
        private bool wasDrawingDebug = false;

        // Store renderers we disable so we can re-enable later
        private readonly List<Renderer> hiddenRenderers = new List<Renderer>();

        // Cache of each bone’s original local rotation (so offsets aren’t cumulative)
        private readonly Dictionary<string, Quaternion> boneBaseRotations = new Dictionary<string, Quaternion>();

        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////

        public Animator Animator => animator;

        #endregion

        #region INITIALIZATION .........................................................................................

        public override void EditModeInitialize()
        {
            base.EditModeInitialize();
            description = "Humanoid Avatar Animator.";
            animator = this.GetOrAddComponent<Animator>();
        }

        #endregion

        #region MONOBEHAVIOURS .........................................................................................

        protected override void OnUpdate()
        {
            base.OnUpdate();
            animator.enabled = active;
        }

        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
            
            if (!active) return;
            
            //ApplyRotationOffsets();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!active) return;
            
            //ApplyFootIK(AvatarIKGoal.LeftFoot, AvatarIKHint.LeftKnee);
            //ApplyFootIK(AvatarIKGoal.RightFoot, AvatarIKHint.RightKnee);
        }

        #endregion

        #region GENERAL ................................................................................................

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

            animator.runtimeAnimatorController = CharacterSettings.animatorController;
            
            AnimatorOverrideController overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);

            ApplyOverrideAnimation(config.idle, AnimatorState.Idle, overrides);
            ApplyOverrideAnimation(config.inAir, AnimatorState.InAir, overrides);
            ApplyOverrideAnimation(config.jumpStart, AnimatorState.JumpStart, overrides);
            ApplyOverrideAnimation(config.jumpLand, AnimatorState.JumpLand, overrides);
            ApplyOverrideAnimation(config.jumpLandWalk, AnimatorState.JumpLandWalk, overrides);
            ApplyOverrideAnimation(config.jumpLandRun, AnimatorState.JumpLandRun, overrides);
            ApplyOverrideAnimation(config.crouch, AnimatorState.Crouch, overrides);
            ApplyOverrideAnimation(config.walk, AnimatorState.Walk, overrides);
            ApplyOverrideAnimation(config.run, AnimatorState.Run, overrides);

            overrideController.ApplyOverrides(overrides);
            animator.runtimeAnimatorController = overrideController;
            animator.Rebind();

            // Disabled Bone Rotation for now, was not working as well as required, decided to stop burning time for now!.
            // 2) Cache each bone’s base localRotation so offsets aren’t cumulative
            //CacheBoneBaseRotations();

            // 3) Apply any spotlight logic (unchanged)
            EnvironmentManager.Instance.ApplyAvatarLighting(this);
        }

        private void ApplyOverrideAnimation(AnimationClip clip, AnimatorState state,
            List<KeyValuePair<AnimationClip, AnimationClip>> overrides)
        {
            if (clip == null) return;
            var overrideState = overrides.Find(i => i.Key.name == state.ToString());
            int index = overrides.IndexOf(overrideState);
            if (index >= 0)
            {
                overrides[index] = new KeyValuePair<AnimationClip, AnimationClip>(overrideState.Key, clip);
            }
        }

        /// <summary>
        /// Updates the animator state based on the provided parameters in the AvatarAnimatorState.
        /// </summary>
        public void UpdateAnimatorState(AvatarAnimatorState state)
        {
            if (animator == null) return;

            bool isMoving = state.currentSpeed > 0.001f;
            animator.SetFloat(SpeedID, isMoving ? state.currentSpeed : 0f);

            bool isFalling = !state.isGrounded && state.verticalVelocity < 0f;

            animator.SetBool(JumpID, state.jump);
            animator.SetBool(CrouchID, state.crouch);
            animator.SetBool(DanceID, state.dance);
            animator.SetBool(GroundedID, state.isGrounded);
            animator.SetBool(FreeFallID, isFalling);

            float motionSpeed = isMoving && state.maxSpeed > 0f
                ? Mathf.Clamp01(state.currentSpeed / state.maxSpeed)
                : 1f;
            animator.SetFloat(MotionSpeedID, motionSpeed);
        }

        #endregion

        #region FOOT IK ................................................................................................

        

        #endregion

        #region DEBUG GIZMOS + RENDERER TOGGLE ...........................................................................

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // 1) Toggle all child renderers ON/OFF once when drawDebugGizmos flips
            HandleRendererToggle();

            // 2) If gizmos are off or no avatarConfig, we’re done
            if (!drawDebugGizmos || avatarConfig == null) return;

            // 3) Draw bold yellow line for every bone in BoneMapping
            if (avatarConfig.BoneMapping != null)
            {
                foreach (var map in avatarConfig.BoneMapping)
                {
                    string boneName = avatarConfig.GetBoneTarget(map.BoneID);
                    if (string.IsNullOrEmpty(boneName))
                        continue;

                    Transform bone = transform.FindChildTransform(boneName);
                    if (bone == null)
                        continue;

                    // Compute “length” to first child (or fallback)
                    float boneLength = 0.1f;
                    if (bone.childCount > 0)
                    {
                        boneLength = Vector3.Distance(bone.position, bone.GetChild(0).position);
                    }

                    boneLength = Mathf.Max(boneLength, 0.2f);

                    // Draw yellow line along bone’s local “up” direction
                    Gizmos.color = Color.yellow;
                    Vector3 worldUp = bone.TransformDirection(Vector3.up);
                    Gizmos.DrawLine(bone.position, bone.position + worldUp * boneLength);

                    if (string.IsNullOrEmpty(boneName)) continue;

                    if (bone == null) continue;

                    // a) Draw white label just above the bone’s position
                    /*GUIStyle style = new GUIStyle
                    {
                        normal = { textColor = Color.white },
                        fontSize = 12
                    };
                    Vector3 labelPos = bone.position + Vector3.up * 0.02f;
                    Handles.Label(labelPos, boneName, style);*/

                    // b) Draw three colored axes from that same position
                    //    using bone.TransformDirection(…) so it shows exactly “base + offset”—
                    //    no extra multiplication by offsetQuat needed.

                    // Determine a base length (distance to first child, or fallback)
                    float baseLength = 0.1f;
                    if (bone.childCount > 0)
                    {
                        baseLength = Vector3.Distance(bone.position, bone.GetChild(0).position);
                    }

                    float axisLen = Mathf.Max(baseLength * 0.5f, 0.1f);

                    // X‐axis (red)
                    Vector3 xAxis = bone.TransformDirection(Vector3.right);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(bone.position, bone.position + xAxis * axisLen);

                    // Y‐axis (green)
                    Vector3 yAxis = bone.TransformDirection(Vector3.up);
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(bone.position, bone.position + yAxis * axisLen);

                    // Z‐axis (blue)
                    Vector3 zAxis = bone.TransformDirection(Vector3.forward);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(bone.position, bone.position + zAxis * axisLen);
                }
            }
        }

        private void HandleRendererToggle()
        {
            if (drawDebugGizmos && !wasDrawingDebug)
            {
                HideAllChildRenderers();
                wasDrawingDebug = true;
            }
            else if (!drawDebugGizmos && wasDrawingDebug)
            {
                ShowAllChildRenderers();
                wasDrawingDebug = false;
            }
        }

        private void HideAllChildRenderers()
        {
            hiddenRenderers.Clear();
            Renderer[] allRenderers = GetComponentsInChildren<Renderer>(includeInactive: true);
            foreach (Renderer r in allRenderers)
            {
                if (r.enabled)
                {
                    r.enabled = false;
                    hiddenRenderers.Add(r);
                }
            }
        }

        private void ShowAllChildRenderers()
        {
            foreach (Renderer r in hiddenRenderers)
            {
                if (r != null)
                    r.enabled = true;
            }
            hiddenRenderers.Clear();
        }
#endif

        #endregion
    }
    
}
