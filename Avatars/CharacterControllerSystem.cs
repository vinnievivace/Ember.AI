using EmberAI.Avatars;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterControllerSystem : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] CharacterSettings settings;
    [SerializeField] MonoBehaviour      inputModule;    // must implement ICharacterInput

    [Header("Animation")]
    [SerializeField] Animator           animator;       // assign your Animator here

    [Header("Rotation")]
    [Tooltip("Degrees per second to turn toward movement direction")]
    [SerializeField] float              rotationSpeed = 720f;

    [Header("Camera")]
    [Tooltip("If null, will use Camera.main")]
    [SerializeField] Transform          cameraTransform;

    // Animator parameter hashes
    static readonly int SpeedHash       = Animator.StringToHash("Speed");
    static readonly int JumpHash        = Animator.StringToHash("Jump");
    static readonly int GroundedHash    = Animator.StringToHash("Grounded");
    static readonly int FreeFallHash    = Animator.StringToHash("FreeFall");
    static readonly int MotionSpeedHash = Animator.StringToHash("MotionSpeed");

    ICharacterInput     _input;
    CharacterController _cc;
    float               _verticalVelocity;

    void Awake()
    {
        _cc    = GetComponent<CharacterController>();
        _input = inputModule as ICharacterInput;
        if (_input == null)
            Debug.LogError("Input Module must implement ICharacterInput");

        if (animator == null)
            Debug.LogWarning("Animator reference is null—no animation will be driven.");

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // 1) Read raw 2D input (already normalized)
        Vector2 move2D = _input.ReadMovementInput();
        bool    run    = _input.IsRunning();
        bool    crouch = settings.canCrouch && _input.IsCrouching();
        bool    jump   = settings.canJump  && _input.JumpRequested();

        // Early-out if no camera
        if (cameraTransform == null)
        {
            Debug.LogError("No cameraTransform assigned and Camera.main is null!");
            return;
        }

        // 2) Build camera-relative basis
        Vector3 camFwd = cameraTransform.forward;
        camFwd.y = 0;
        camFwd.Normalize();

        Vector3 camRight = cameraTransform.right;
        camRight.y = 0;
        camRight.Normalize();

        // 3) Desired world movement direction
        Vector3 desiredDir = camFwd * move2D.y + camRight * move2D.x;

        // 4) Rotate to face that direction
        if (desiredDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(desiredDir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }

        // 5) Decide move speed
        float speed = crouch   ? settings.crouchSpeed
                    : run     ? settings.runSpeed
                              : settings.walkSpeed;

        // 6) Build final movement vector
        Vector3 move = desiredDir.normalized * speed;

        // 7) Handle jump + gravity
        if (_cc.isGrounded)
        {
            _verticalVelocity = -0.1f;
            if (jump)
                _verticalVelocity = settings.jumpForce;
        }
        _verticalVelocity += settings.gravity * Time.deltaTime;
        move.y = _verticalVelocity;

        // 8) Apply motion
        _cc.Move(move * Time.deltaTime);

        // 9) Smoothly adjust crouch height (optional)
        float targetHeight = crouch ? settings.crouchHeight : settings.runSpeed;
        _cc.height = Mathf.Lerp(_cc.height, targetHeight, 10f * Time.deltaTime);

        // 10) Drive Animator parameters
        if (animator)
        {
            // TODO understand speed, vs motionspeed.
            
            animator.SetFloat(SpeedHash, move2D.magnitude == 0 ? 0 : speed);
            animator.SetBool (JumpHash,  jump);
            animator.SetBool (GroundedHash, _cc.isGrounded);
            bool freeFall = !_cc.isGrounded && _verticalVelocity < 0f;
            animator.SetBool (FreeFallHash, freeFall);
            // MotionSpeed can be full world‑space speed
            animator.SetFloat(MotionSpeedHash, move2D.magnitude);
        }
    }
}
