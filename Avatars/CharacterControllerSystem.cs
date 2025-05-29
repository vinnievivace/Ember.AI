using EmberAI.Attributes;
using EmberAI.Core;
using UnityEngine;

namespace EmberAI.Avatars
{
    [RequireComponent(typeof(CharacterController), typeof(Animator))]
    public class CharacterControllerSystem : EmberBehaviour
    {
        // Animator parameter hashes
        private static readonly int SpeedHash       = Animator.StringToHash("Speed");
        private static readonly int JumpHash        = Animator.StringToHash("Jump");
        private static readonly int GroundedHash    = Animator.StringToHash("Grounded");
        private static readonly int FreeFallHash    = Animator.StringToHash("FreeFall");
        private static readonly int MotionSpeedHash = Animator.StringToHash("MotionSpeed");

        // Settings
        [BoxGroup("Settings"), SerializeField] 
        private CharacterSettings settings;
        
        // State
        [BoxGroup("State")] public bool active = true;

        // Components
        [BoxGroup("Components")] public BaseCharacterInput characterInput;
        [BoxGroup("Components"), SerializeField] private Animator animator;
        [BoxGroup("Components"), SerializeField] private CharacterController controller;

        // Runtime
        private Transform cameraTransform;
        private float verticalVelocity;

        #region Initialization

        public override void InitializeDependencies()
        {
            base.InitializeDependencies();
            controller     = this.GetOrAddComponent<CharacterController>();
            animator       = this.GetOrAddComponent<Animator>();
            characterInput = GetComponent<BaseCharacterInput>();
            
            // Default capsule
            controller.center = new Vector3(0f, 0.5f, 0f);
            controller.height = 1f;

            ApplySettings();
        }

        public override void DestroyDependencies()
        {
            base.DestroyDependencies();
            this.RemoveComponent<CharacterController>();
            this.RemoveComponent<Animator>();
            this.RemoveComponent<BaseCharacterInput>();
        }

        #endregion

        #region MonoBehaviour

        protected override void OnAwake()
        {
            base.OnAwake();
            cameraTransform = Camera.main?.transform;
            if (cameraTransform == null)
                Debug.LogError("No Camera.main found for direction calculation.");
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            
            if(settings.updateType == UpdateMode.Update) ApplyUpdates(Time.deltaTime);
        }

        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
            
            if(settings.updateType == UpdateMode.LateUpdate) ApplyUpdates(Time.fixedDeltaTime);
        }

        protected override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            
            if(settings.updateType == UpdateMode.FixedUpdate) ApplyUpdates(Time.fixedDeltaTime);
        }
        
        #endregion
        
        #region General ................................................................................................
        
        private void ApplyUpdates(float delta)
        {
            controller.enabled = active;
            if (!active) return;

            Vector2 moveInput   = characterInput.ReadMovementInput();
            bool    isRunning   = characterInput.IsRunning();
            bool    isCrouching = settings.canCrouch && characterInput.IsCrouching();
            bool    jumpRequested = settings.canJump && characterInput.JumpRequested();

            Vector3 moveDir = CalculateMoveDirection(moveInput);
            RotateTowards(moveDir);

            float  targetSpeed = isCrouching ? settings.crouchSpeed : isRunning   ? settings.runSpeed : settings.walkSpeed;
            bool isMoving = moveInput.sqrMagnitude > 0f;
            bool wasGrounded = controller.isGrounded;

            if (wasGrounded)
            {
                // snap to ground
                verticalVelocity = - settings.groundStick;
                
                if (jumpRequested)
                {
                    verticalVelocity = settings.jumpForce;
                }
            }
            else
            {
                verticalVelocity += settings.gravity * delta;
            }

            Vector3 groundNormal = Vector3.up;
            
            if (wasGrounded)
            {
                groundNormal = SampleGroundNormal();
            }

            Vector3 horizontal = Vector3.ProjectOnPlane(moveDir.normalized * targetSpeed, groundNormal);
            Vector3 velocity   = horizontal + Vector3.up * verticalVelocity;

            controller.Move(velocity * Time.deltaTime);

            bool isGroundedNow = controller.isGrounded;
            bool didJump       = jumpRequested && wasGrounded;
            
            UpdateAnimator(isMoving, targetSpeed, didJump, isGroundedNow, moveInput.magnitude);
        }
        
        #endregion
        
        #region Animation Events .......................................................................................
        
        private void OnFootstep(AnimationEvent animationEvent)
        {
            /*if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }*/
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            /*if (animationEvent.animatorClipInfo.weight > 0.5f && LandingAudioClip != null)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }*/
        }
        
        #endregion
        
        #region Movement calculations ..................................................................................

        private Vector3 CalculateMoveDirection(Vector2 input)
        {
            Vector3 fwd = cameraTransform.forward; fwd.y = 0; fwd.Normalize();
            Vector3 right = cameraTransform.right; right.y = 0; right.Normalize();
            return fwd * input.y + right * input.x;
        }

        private void RotateTowards(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.001f) return;
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, settings.rotationSpeed * Time.deltaTime);
        }

        private Vector3 SampleGroundNormal()
        {
            // simple raycast down from center
            Vector3 origin = transform.position + Vector3.up * (controller.height * 0.5f);
            if (Physics.Raycast(origin, Vector3.down, out var hit, controller.height * 0.5f + 0.1f, settings.groundLayer))
                return hit.normal;
            return Vector3.up;
        }

        private void UpdateAnimator(bool isMoving, float speed, bool jumped, bool isGrounded, float inputMag)
        {
            if (animator == null) return;

            animator.SetFloat(SpeedHash,     isMoving ? speed : 0f);
            animator.SetBool(JumpHash,       jumped);
            animator.SetBool(GroundedHash,   isGrounded);

            bool isFalling = !isGrounded && verticalVelocity < 0f;
            animator.SetBool(FreeFallHash, isFalling);

            float motionSpeed = isMoving ? inputMag : settings.idleAnimationSpeed;
            animator.SetFloat(MotionSpeedHash, motionSpeed);
        }

        private void ApplySettings()
        {
            if (settings == null)
            {
                Debug.LogError("CharacterSettings missing on CharacterControllerSystem.");
                return;
            }
            animator.runtimeAnimatorController = settings.animatorController;
            animator.applyRootMotion             = settings.useRootMotion;
        }

        public void ApplyAvatar(Avatar avatar)
        {
            animator.avatar                     = avatar;
            animator.runtimeAnimatorController = settings.animatorController;
            animator.Rebind();
        }

        #endregion
    }
}
