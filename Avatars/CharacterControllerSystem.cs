using System;
using EmberAI.Attributes;
using EmberAI.Core;
using EmberAI.Envrionment;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EmberAI.Avatars
{
    public class CharacterControllerSystem : EmberBehaviour
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        private bool _avatarInitialized;
        private Transform _cameraTransform;
        private float     _verticalVelocity;
        
        // Tracks current horizontal speed and decel state
        private float _currentSpeed       = 0f;
        private float _stopSpeed          = 0f;
        private float _decelElapsed       = 0f;
        private bool  _decelerating       = false;
        private bool  _wasMovingLastFrame = false;

        // We need to remember “what maxSpeed we were aiming at” when we last moved:
        private float _lastRawTarget   = 0f;  // walkSpeed/runSpeed/crouchSpeed from last moving frame
        private float _stopRawTarget   = 0f;  // “rawTarget” stored the moment deceleration began

        // Store last move direction so we can keep moving that way while decelerating:
        private Vector3 _lastMoveDirection = Vector3.zero;
        
        // Store the current IK local rotation override for head:
        private Quaternion _headIKRotation = Quaternion.identity;

        // Buffer for jump input so it isn't lost mid‐air
        private bool _jumpRequestedCached = false;

        [BoxGroup("Settings"), SerializeField]
        private CharacterSettings settings;
        
        [BoxGroup("Audio"), SerializeField]
        private float animationEventVolume = 1f;

        [BoxGroup("Audio"), SerializeField]
        private AudioClip footStep, footStepAlt, landJump;

        [BoxGroup("Look At"), SerializeField]
        public bool lookAtEnabled = true;
        
        [BoxGroup("Look At"), SerializeField]
        private Transform lookAtTarget;
        
        [BoxGroup("Loo At")]
        public Vector3 lookAtOffset = new Vector3(0f, 0f, 0f);
        
        [BoxGroup("Look At"), SerializeField]
        private float lookAtSpeed = 8, lookAtHorizontalClamp = 60, lookAtVerticalClamp = 50;
        
        [BoxGroup("Components")]
        public BaseCharacterInput characterInput;

        [BoxGroup("Components"), SerializeField]
        private AvatarAnimator     avatarAnimator;

        [BoxGroup("Components"), SerializeField]
        private CharacterController controller;

        [BoxGroup("Components"), SerializeField]
        private AudioSource        audioSource;

        [BoxGroup("State")]
        public bool active = true;

        [BoxGroup("Debug"), ReadOnly, SerializeField]
        private Transform headTransform;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        public bool HasInput { get; private set; }
        
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

            controller     = this.GetOrAddComponent<CharacterController>();
            avatarAnimator = this.GetOrAddComponent<AvatarAnimator>();
            audioSource    = this.GetOrAddComponent<AudioSource>();
            characterInput = GetComponent<BaseCharacterInput>();

            // Default capsule
            controller.center = new Vector3(0f, 0.5f, 0f);
            controller.height = 1f;

            // (You can set slopeLimit in the Inspector or here:
            //  controller.slopeLimit = settings.slopeLimit; // if you expose it in CharacterSettings
            
            
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
                Debug.LogError("[CCS] No Camera.main found.");
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            
            avatarAnimator.active = active;

            // Buffer jump input here—only set flag, don't consume yet
            if (settings.canJump && characterInput.JumpTriggered()) _jumpRequestedCached = true;
            
            if (settings.updateMode == UpdateMode.Update) ApplyInputs(Time.deltaTime);
        }

        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
            
            if (settings.updateMode == UpdateMode.LateUpdate) ApplyInputs(Time.deltaTime);
        }
        
        protected override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            
            if (settings.updateMode == UpdateMode.FixedUpdate) ApplyInputs(Time.fixedDeltaTime);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!lookAtEnabled) return;
            if (avatarAnimator == null || lookAtTarget == null || headTransform == null) return;

            // Apply offset in world space
            Vector3 targetPosition = lookAtTarget.position + lookAtOffset;

            // Calculate world direction from head to target
            Vector3 worldDir = targetPosition - headTransform.position;
            if (worldDir.sqrMagnitude < 0.0001f) return;

            // Convert to head's local space
            Quaternion headRot = headTransform.rotation;
            Vector3 localDir = Quaternion.Inverse(headRot) * worldDir.normalized;

            // Compute local Euler angles
            Vector3 localEuler = Quaternion.LookRotation(localDir, Vector3.up).eulerAngles;
            localEuler.x = (localEuler.x > 180f) ? localEuler.x - 360f : localEuler.x;
            localEuler.y = (localEuler.y > 180f) ? localEuler.y - 360f : localEuler.y;

            // Clamp within horizontal and vertical angles
            float clampedYaw   = Mathf.Clamp(localEuler.y, -lookAtHorizontalClamp, lookAtHorizontalClamp);
            float clampedPitch = Mathf.Clamp(localEuler.x, -lookAtVerticalClamp, lookAtVerticalClamp);

            bool withinYaw   = Mathf.Abs(localEuler.y) <= lookAtHorizontalClamp;
            bool withinPitch = Mathf.Abs(localEuler.x) <= lookAtVerticalClamp;

            Quaternion targetLocal = (withinYaw && withinPitch)
                ? Quaternion.Euler(clampedPitch, clampedYaw, 0f)
                : Quaternion.identity;

            // Smoothly interpolate stored IK rotation toward targetLocal
            float factor = lookAtSpeed * Time.deltaTime;
            _headIKRotation = Quaternion.Slerp(_headIKRotation, targetLocal, factor);

            // Apply via Animator IK
            avatarAnimator.Animator.SetBoneLocalRotation(HumanBodyBones.Head, _headIKRotation);
        }

        
        #endregion

        #region General ................................................................................................

        public void ApplyAvatar(Avatar avatar, AvatarConfig config)
        {
            if (config == null) return;
            
            avatarAnimator.InitializeAvatar(avatar, config);

            if (config.landJump    != null) landJump    = config.landJump;
            if (config.footstep    != null) footStep    = config.footstep;
            if (config.footstepAlt != null) footStepAlt = config.footstepAlt;

            headTransform = transform.FindChildTransform(config.GetBoneTarget(AvatarBoneID.Head));
            _headIKRotation = Quaternion.identity;
            
            _avatarInitialized = true;
            active = true;
        }
        
        private void ApplySettings()
        {
            if (settings == null)
            {
                Debug.LogError("[CCS] Missing CharacterSettings.");
                return;
            }
            avatarAnimator.InitializeAnimator(settings);

            // If you want to drive slopeLimit from CharacterSettings, uncomment below:
            // controller.slopeLimit = settings.slopeLimit;
            // controller.stepOffset = settings.stepOffset; 
        }

        private void ApplyInputs(float delta)
        {
            // until Avatar is initialized, we can't do anything
            if (!_avatarInitialized)
            {
                active = false;
                controller.enabled = false;
                return;
            }
            
            controller.enabled = active;
            
            if (!active) return;
            
            Vector2 moveInput   = characterInput.ReadMovementInput();
            bool    isRunning   = characterInput.IsRunning();
            bool    isCrouching = settings.canCrouch && characterInput.IsCrouching();
            bool    rawMoving   = moveInput.sqrMagnitude > 0f;

            // Determine if we can consume the buffered jump this frame:
            bool wasGrounded = controller.isGrounded;
            bool jumpRequestedThisFrame = false;
            
            if (settings.canJump && _jumpRequestedCached && wasGrounded)
            {
                jumpRequestedThisFrame = true;
                _jumpRequestedCached = false;  // consume only when grounded
            }

            float rawTarget = 0f;
            
            if (rawMoving)
            {
                rawTarget = isCrouching ? settings.crouchSpeed : (isRunning ? settings.runSpeed : settings.walkSpeed);
                _lastRawTarget = rawTarget;
            }

            if (!rawMoving && _wasMovingLastFrame && !_decelerating)
            {
                _decelerating     = true;
                _decelElapsed     = 0f;
                _stopSpeed        = _currentSpeed;
                _stopRawTarget    = _lastRawTarget;
            }

            if (rawMoving && _decelerating) 
                _decelerating = false;

            _wasMovingLastFrame = rawMoving;

            Vector3 moveDir = CalculateMoveDirection(moveInput);
            
            if (rawMoving)
            {
                _lastMoveDirection = moveDir.normalized;
            }
            
            RotateTowards(rawMoving ? moveDir : _lastMoveDirection);

            if (_decelerating)
            {
                _decelElapsed += delta;
                float t = settings.decelerationTime > 0f 
                    ? Mathf.Clamp01(_decelElapsed / settings.decelerationTime) 
                    : 1f;
                
                _currentSpeed = Mathf.Lerp(_stopSpeed, 0f, t);
                if (t >= 1f) 
                    _decelerating = false;
            }
            else
            {
                _currentSpeed = Mathf.MoveTowards(
                    _currentSpeed, 
                    rawTarget, 
                    settings.accelerationSpeed * delta
                );
            }

            if (wasGrounded)
            {
                _verticalVelocity = -settings.groundStick;
                if (jumpRequestedThisFrame)
                {
                    _verticalVelocity = settings.jumpForce;
                }
            }
            else
            {
                _verticalVelocity += settings.gravity * delta;
            }

            // ───────────── Enforce slopeLimit here ─────────────
            Vector3 appliedDirection = _decelerating ? _lastMoveDirection : (rawMoving ? moveDir.normalized : Vector3.zero);

            // Sample ground normal
            Vector3 groundNormal = wasGrounded ? SampleGroundNormal() : Vector3.up;
            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);

            Vector3 horizontal;

            if (wasGrounded && slopeAngle > controller.slopeLimit)
            {
                // Too steep to climb: zero out horizontal movement so you don't ascend
                horizontal = Vector3.zero;
                
                // Optionally, slide down if you want:
                // Vector3 downSlopeDir = new Vector3(groundNormal.x, -groundNormal.y, groundNormal.z);
                // horizontal = downSlopeDir.normalized * settings.slideSpeed; 
                
                // Also mark as not grounded if you want to start falling:
                // wasGrounded = false;
            }
            else
            {
                // Project movement onto the plane of the ground normal
                horizontal = Vector3.ProjectOnPlane(appliedDirection * _currentSpeed, groundNormal);
            }

            Vector3 velocity = horizontal + Vector3.up * _verticalVelocity;
            controller.Move(velocity * delta);

            AvatarAnimatorState state = new AvatarAnimatorState();
            
            state.maxSpeed        = _decelerating ? _stopRawTarget : rawTarget;
            state.isGrounded      = controller.isGrounded;
            state.jump            = jumpRequestedThisFrame;
            state.crouch          = isCrouching && state.isGrounded;
            state.dance           = characterInput.IsDancing();
            state.currentSpeed    = _currentSpeed;
            state.verticalVelocity = _verticalVelocity;

            avatarAnimator.UpdateAnimatorState(state);

            HasInput = state.currentSpeed != 0 || state.dance || state.jump || state.crouch;;
        }
        
        private Vector3 CalculateMoveDirection(Vector2 input)
        {
            Vector3 fwd   = _cameraTransform.forward; fwd.y = 0; fwd.Normalize();
            Vector3 right = _cameraTransform.right;   right.y = 0; right.Normalize();
            return fwd * input.y + right * input.x;
        }

        private void RotateTowards(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.001f) 
                return;

            Quaternion target = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, 
                target, 
                settings.rotationSpeed * Time.deltaTime
            );
        }

        private Vector3 SampleGroundNormal()
        {
            Vector3 origin = transform.position + Vector3.up * (controller.height * 0.5f);
            if (Physics.Raycast(
                    origin, 
                    Vector3.down, 
                    out RaycastHit hit, 
                    controller.height * 0.5f + 0.1f, EnvironmentManager.Instance.GroundLayer))
            {
                return hit.normal;
            }
            return Vector3.up;
        }
        
        #endregion
        
        #region Animation Events .......................................................................................

        [UsedImplicitly]
        private void OnFootstep(AnimationEvent e)
        {
            if (footStep == null && footStepAlt == null) return;
            
            AudioClip clip = (footStep != null && footStepAlt != null) ? (Random.value < 0.5f ? footStep : footStepAlt) : (footStep ?? footStepAlt);
            
            AudioSource.PlayClipAtPoint(clip, transform.TransformPoint(controller.center), animationEventVolume);
        }

        [UsedImplicitly]
        private void OnLand(AnimationEvent e)
        {
            if (landJump == null) return;
            
            if (e.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(landJump, transform.TransformPoint(controller.center), animationEventVolume);
            }
        }

        #endregion

        #region Event Handlers .........................................................................................

        #endregion

        #endregion
        
    }
}
