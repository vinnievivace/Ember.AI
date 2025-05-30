using EmberAI.Attributes;
using EmberAI.Core;
using EmberAI.UI;
using JetBrains.Annotations;
using UnityEngine;

namespace EmberAI.Avatars
{
    public class CharacterControllerSystem : EmberBehaviour
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        // Animator parameter hashes
        /*private static readonly int SpeedHash       = Animator.StringToHash("Speed");
        private static readonly int JumpHash        = Animator.StringToHash("Jump");
        private static readonly int GroundedHash    = Animator.StringToHash("Grounded");
        private static readonly int FreeFallHash    = Animator.StringToHash("FreeFall");
        private static readonly int MotionSpeedHash = Animator.StringToHash("MotionSpeed");*/
        
        private Transform _cameraTransform;
        private float _verticalVelocity;

        [BoxGroup("Settings"), SerializeField] 
        private CharacterSettings settings;
        
        [BoxGroup("Audio"), SerializeField] 
        private float animationEventVolume = 1f;
        
        [BoxGroup("Audio"), SerializeField] 
        private AudioClip footStep, footStepAlt, landJump;
        
        [BoxGroup("Components")] 
        public BaseCharacterInput characterInput;
        
        [BoxGroup("Components"), SerializeField] 
        private AvatarAnimator avatarAnimator;
        
        [BoxGroup("Components"), SerializeField] 
        private CharacterController controller;
        
        [BoxGroup("Components"), SerializeField] 
        private AudioSource audioSource;
        
        [BoxGroup("State")] 
        public bool active = true;
    
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
            
            controller = this.GetOrAddComponent<CharacterController>();
            avatarAnimator = this.GetOrAddComponent<AvatarAnimator>();
            audioSource = this.GetOrAddComponent<AudioSource>();
            
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

        #region MonoBehaviours .........................................................................................

        protected override void OnAwake()
        {
            base.OnAwake();
            _cameraTransform = Camera.main?.transform;
            if (_cameraTransform == null)
                Debug.LogError("No Camera.main found for direction calculation.");
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            
            avatarAnimator.active = active;
            
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
            if (UIManager.Instance != null) active = !UIManager.Instance.UIInteraction;
            
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
                _verticalVelocity = - settings.groundStick;
                
                if (jumpRequested)
                {
                    _verticalVelocity = settings.jumpForce;
                }
            }
            else
            {
                _verticalVelocity += settings.gravity * delta;
            }

            Vector3 groundNormal = Vector3.up;
            
            if (wasGrounded)
            {
                groundNormal = SampleGroundNormal();
            }

            Vector3 horizontal = Vector3.ProjectOnPlane(moveDir.normalized * targetSpeed, groundNormal);
            Vector3 velocity   = horizontal + Vector3.up * _verticalVelocity;

            controller.Move(velocity * Time.deltaTime);

            bool isGroundedNow = controller.isGrounded;
            bool didJump       = jumpRequested && wasGrounded;
            
            avatarAnimator.UpdateAnimatorParams(isMoving, targetSpeed, didJump, isGroundedNow, moveInput.magnitude, _verticalVelocity);
        }
        
        #endregion
        
         #region Animation Events .......................................................................................
        
        [UsedImplicitly]
        private void OnFootstep(AnimationEvent animationEvent)
        {
            if(footStep == null && footStepAlt == null) return;

            AudioClip footStepClip;
            
            if(footStep != null && footStepAlt != null)
            {
                footStepClip = Random.Range(0, 1) == 0 ? footStep : footStepAlt;
            }
            else if (footStep != null)
            {
                footStepClip = footStep;
            }
            else
            {
                footStepClip = footStepAlt;
            }
            
            AudioSource.PlayClipAtPoint(footStepClip, transform.TransformPoint(controller.center), animationEventVolume);
        }

        [UsedImplicitly]
        private void OnLand(AnimationEvent animationEvent)
        {
            if(landJump == null) return;
            
            if (animationEvent.animatorClipInfo.weight > 0.5f && landJump != null)
            {
                AudioSource.PlayClipAtPoint(landJump, transform.TransformPoint(controller.center), animationEventVolume);
            }
        }
        
        #endregion
        
        #region Movement calculations ..................................................................................

        private Vector3 CalculateMoveDirection(Vector2 input)
        {
            Vector3 fwd = _cameraTransform.forward; fwd.y = 0; fwd.Normalize();
            Vector3 right = _cameraTransform.right; right.y = 0; right.Normalize();
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

        private void ApplySettings()
        {
            if (settings == null)
            {
                Debug.LogError("CharacterSettings missing on CharacterControllerSystem.");
                return;
            }
            
            avatarAnimator.InitializeAnimator(settings);;
        }

        public void ApplyAvatar(Avatar avatar, AvatarConfig config)
        {
            avatarAnimator.InitializeAvatar(avatar, config);
            
            if(config == null) return;

            if (config.landJump != null) landJump = config.landJump;
            if(config.footstep != null) footStep = config.footstep;
            if(config.footstepAlt != null) footStepAlt = config.footstepAlt;
            
            
        }

        #endregion

        #region Event Handlers .........................................................................................

        #endregion

        #endregion
    }
}
