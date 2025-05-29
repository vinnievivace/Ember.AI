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
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");
        private static readonly int FreeFallHash = Animator.StringToHash("FreeFall");
        private static readonly int MotionSpeedHash = Animator.StringToHash("MotionSpeed");

        // Settings
        [BoxGroup("Settings"), SerializeField]
        private CharacterSettings settings;
        [BoxGroup("Settings"), SerializeField, Tooltip("Layers considered as ground.")]
        private LayerMask groundLayer;
        [BoxGroup("Settings"), SerializeField, Tooltip("Distance to check for ground detection.")]
        private float groundCheckDistance = 0.2f;
        [BoxGroup("Settings"), SerializeField, Tooltip("Downward velocity when grounded to keep snapped.")]
        private float groundStick = 2f;
        [BoxGroup("Settings"), SerializeField, Tooltip("Animation playback speed when idle.")]
        private float idleAnimationSpeed = 1f;

        // State
        [BoxGroup("State")]
        public bool active = true;

        // Components
        [BoxGroup("Components")]
        public BaseCharacterInput characterInput;
        [BoxGroup("Components"), SerializeField]
        private Animator animator;
        [BoxGroup("Components"), SerializeField]
        private CharacterController controller;

        // Runtime variables
        private Transform cameraTransform;
        private float verticalVelocity;

        #region Initialization

        public override void InitializeDependencies()
        {
            base.InitializeDependencies();
            controller = this.GetOrAddComponent<CharacterController>();
            animator = this.GetOrAddComponent<Animator>();
            characterInput = GetComponent<BaseCharacterInput>();

            // Default capsule settings
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

        #region MonoBehaviour Overrides

        protected override void OnAwake()
        {
            base.OnAwake();
            cameraTransform = Camera.main != null ? Camera.main.transform : null;

            if (characterInput == null)
                Debug.LogError("Input Module must implement BaseCharacterInput");
        }

        protected override void OnUpdate()
        {
            controller.enabled = active;
            if (!active) return;

            // Read inputs
            Vector2 moveInput = characterInput.ReadMovementInput();
            bool isRunning = characterInput.IsRunning();
            bool isCrouching = settings.canCrouch && characterInput.IsCrouching();
            bool wantsJump = settings.canJump && characterInput.JumpRequested();

            if (cameraTransform == null)
            {
                Debug.LogError("No cameraTransform assigned and Camera.main is null!");
                return;
            }

            // Calculate movement direction and rotation
            Vector3 moveDirection = CalculateMoveDirection(moveInput);
            RotateTowards(moveDirection);

            // Determine movement speed
            float targetSpeed = DetermineSpeed(isRunning, isCrouching);
            bool isMoving = moveInput.sqrMagnitude > 0f;

            // Ground check and physics
            bool isGrounded = CheckGround(out Vector3 groundNormal);
            ApplyGravityAndJump(wantsJump, isGrounded);

            // Move the character
            Vector3 finalMovement = ProjectMovement(moveDirection, targetSpeed, groundNormal);
            controller.Move(finalMovement * Time.deltaTime);

            // Update animator parameters
            UpdateAnimator(isMoving, targetSpeed, wantsJump, isGrounded, moveInput.magnitude);
        }

        #endregion

        #region Helpers

        private Vector3 CalculateMoveDirection(Vector2 input)
        {
            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = cameraTransform.right;
            right.y = 0f;
            right.Normalize();

            return forward * input.y + right * input.x;
        }

        private void RotateTowards(Vector3 direction)
        {
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    settings.rotationSpeed * Time.deltaTime
                );
            }
        }

        private float DetermineSpeed(bool isRunning, bool isCrouching)
        {
            if (isCrouching) return settings.crouchSpeed;
            if (isRunning) return settings.runSpeed;
            return settings.walkSpeed;
        }

        private bool CheckGround(out Vector3 groundNormal)
        {
            Vector3 origin = transform.position + controller.center;
            float radius = controller.radius;
            float distance = groundCheckDistance + controller.skinWidth;

            if (Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, distance, groundLayer))
            {
                groundNormal = hit.normal;
                return true;
            }

            groundNormal = Vector3.up;
            return false;
        }

        private void ApplyGravityAndJump(bool wantsJump, bool isGrounded)
        {
            if (isGrounded)
            {
                verticalVelocity = -groundStick;
                if (wantsJump)
                    verticalVelocity = settings.jumpForce;
            }
            else
            {
                verticalVelocity += settings.gravity * Time.deltaTime;
            }
        }

        private Vector3 ProjectMovement(Vector3 direction, float speed, Vector3 groundNormal)
        {
            Vector3 horizontal = direction.normalized * speed;
            Vector3 slopeMovement = Vector3.ProjectOnPlane(horizontal, groundNormal);
            return slopeMovement + Vector3.up * verticalVelocity;
        }

        private void UpdateAnimator(bool isMoving, float speed, bool jumped, bool isGrounded, float inputMagnitude)
        {
            if (animator == null) return;

            // Drive locomotion blend
            animator.SetFloat(SpeedHash, isMoving ? speed : 0f);
            animator.SetBool(JumpHash, jumped);
            animator.SetBool(GroundedHash, isGrounded);

            bool isFalling = !isGrounded && verticalVelocity < 0f;
            animator.SetBool(FreeFallHash, isFalling);

            // Ensure idle animation still plays by preventing zero motion speed
            float motionSpeed = isMoving ? inputMagnitude : idleAnimationSpeed;
            animator.SetFloat(MotionSpeedHash, motionSpeed);
        }

        private void ApplySettings()
        {
            if (settings == null)
            {
                Debug.LogError("No settings assigned!");
                return;
            }

            animator.runtimeAnimatorController = settings.animatorController;
            animator.applyRootMotion = settings.useRootMotion;
        }

        public void ApplyAvatar(Avatar avatar)
        {
            animator.avatar = avatar;
            animator.runtimeAnimatorController = settings.animatorController;
        }

        #endregion
    }
}
