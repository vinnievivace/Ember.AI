using EmberAI;
using EmberAI.Attributes;
using EmberAI.Core;
using UnityEngine;

namespace EmberAI.Avatars
{
    [RequireComponent(typeof(CharacterController), typeof(Animator))]
    public class CharacterControllerSystem : EmberBehaviour
    {
        // Animator parameter hashes
        static readonly int SpeedHash       = Animator.StringToHash("Speed");
        static readonly int JumpHash        = Animator.StringToHash("Jump");
        static readonly int GroundedHash    = Animator.StringToHash("Grounded");
        static readonly int FreeFallHash    = Animator.StringToHash("FreeFall");
        static readonly int MotionSpeedHash = Animator.StringToHash("MotionSpeed");

        // Runtime state
        private float _verticalVelocity;
        private Transform _cameraTransform;

        [BoxGroup("Settings"), SerializeField]
        private CharacterSettings settings;

        [BoxGroup("Settings"), SerializeField, Tooltip("Layers considered as ground.")]
        private LayerMask groundLayer;

        [BoxGroup("Settings"), SerializeField, Tooltip("Distance to check for ground detection.")]
        private float groundCheckDistance = 0.2f;

        [BoxGroup("Settings"), SerializeField, Tooltip("Downward velocity when grounded to keep snapped.")]
        private float groundStick = 2f;

        [BoxGroup("State")] 
        public bool active = true;

        [BoxGroup("Components")]
        public BaseCharacterInput characterInput;

        [BoxGroup("Components"), SerializeField]
        private Animator animator;

        [BoxGroup("Components"), SerializeField]
        private CharacterController _controller;

        #region Initialization

        public override void InitializeDependencies()
        {
            base.InitializeDependencies();
            _controller      = this.GetOrAddComponent<CharacterController>();
            animator         = this.GetOrAddComponent<Animator>();
            characterInput   = GetComponent<BaseCharacterInput>();

            // Default capsule
            _controller.center = new Vector3(0f, 0.5f, 0f);
            _controller.height = 1f;

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

        #region MonoBehaviour Overrides

        protected override void OnAwake()
        {
            base.OnAwake();
            if (_cameraTransform == null && Camera.main != null)
                _cameraTransform = Camera.main.transform;

            if (characterInput == null)
                Debug.LogError("Input Module must implement BaseCharacterInput");
        }

        protected override void OnUpdate()
        {
            _controller.enabled = active;
            
            if(!active) return;
            
            // 1) Read input
            Vector2 move2D = characterInput.ReadMovementInput();
            bool    run    = characterInput.IsRunning();
            bool    crouch = settings.canCrouch && characterInput.IsCrouching();
            bool    jump   = settings.canJump  && characterInput.JumpRequested();

            if (_cameraTransform == null)
            {
                Debug.LogError("No cameraTransform assigned and Camera.main is null!");
                return;
            }

            // 2) Camera basis
            Vector3 camFwd   = _cameraTransform.forward;
            camFwd.y         = 0f;
            camFwd.Normalize();

            Vector3 camRight = _cameraTransform.right;
            camRight.y       = 0f;
            camRight.Normalize();

            // 3) Desired direction & rotation
            Vector3 desiredDir = camFwd * move2D.y + camRight * move2D.x;
            if (desiredDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(desiredDir);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRot,
                    settings.rotationSpeed * Time.deltaTime);
            }

            // 4) Speed selection
            float speed = crouch   ? settings.crouchSpeed
                        : run     ? settings.runSpeed
                                  : settings.walkSpeed;

            // 5) Ground-check (spherecast)
            Vector3 sphereOrigin = transform.position + _controller.center;
            float   sphereRadius = _controller.radius;
            float   rayDist      = groundCheckDistance + _controller.skinWidth;

            bool    isGrounded   = false;
            Vector3 groundNormal = Vector3.up;

            if (Physics.SphereCast(
                sphereOrigin,
                sphereRadius,
                Vector3.down,
                out RaycastHit hit,
                rayDist,
                groundLayer))
            {
                isGrounded   = true;
                groundNormal = hit.normal;
            }

            // 6) Jump & gravity
            if (isGrounded)
            {
                _verticalVelocity = -groundStick;
                if (jump)
                    _verticalVelocity = settings.jumpForce;
            }
            else
            {
                _verticalVelocity += settings.gravity * Time.deltaTime;
            }

            // 7) Movement projection on slope
            Vector3 horizontal = desiredDir.normalized * speed;
            Vector3 slopeMove  = Vector3.ProjectOnPlane(horizontal, groundNormal);

            Vector3 finalMove = slopeMove + Vector3.up * _verticalVelocity;
            _controller.Move(finalMove * Time.deltaTime);

            // 8) Animator updates
            if (animator)
            {
                animator.SetFloat(SpeedHash, move2D.magnitude > 0f ? speed : 0f);
                animator.SetBool(JumpHash,   jump);
                animator.SetBool(GroundedHash, isGrounded);

                bool freeFall = !isGrounded && _verticalVelocity < 0f;
                animator.SetBool(FreeFallHash, freeFall);
                animator.SetFloat(MotionSpeedHash, move2D.magnitude);
            }
        }

        #endregion

        #region Helpers

        private void ApplySettings()
        {
            if (settings == null)
            {
                Debug.LogError("No settings assigned!");
                return;
            }

            animator.runtimeAnimatorController = settings.animatorController;
            animator.applyRootMotion           = settings.useRootMotion;
        }

        public void ApplyAvatar(Avatar avatar)
        {
            animator.avatar                     = avatar;
            animator.runtimeAnimatorController = settings.animatorController;
        }

        // Animation Event callbacks
        public void OnLand()    => Log(LogLevel.Log, name + " landed");
        public void JumpLand()  => Log(LogLevel.Log, name + " landed the jump");
        
        public void OnFootstep() => Log(LogLevel.Log, name + " footstep");

        #endregion
    }
}

