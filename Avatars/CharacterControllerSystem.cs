using EmberAI;
using EmberAI.Attributes;
using EmberAI.Avatars;
using EmberAI.Core;
using UnityEngine;

public class CharacterControllerSystem : EmberBehaviour
{
    #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

    #endregion

    #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

    #endregion

    #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

    // Animator parameter hashes
    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int JumpHash = Animator.StringToHash("Jump");
    static readonly int GroundedHash = Animator.StringToHash("Grounded");
    static readonly int FreeFallHash = Animator.StringToHash("FreeFall");
    static readonly int MotionSpeedHash = Animator.StringToHash("MotionSpeed");
    
    private float _verticalVelocity;
    
    private Transform _cameraTransform;
    
    [BoxGroup("Settings"), SerializeField, OnValueChanged(nameof(ApplySettings))]
    private CharacterSettings settings;
    
    [BoxGroup("Components")]
    public BaseCharacterInput characterInput;    

    [BoxGroup("Components"), SerializeField]
    private Animator animator;
    
    [BoxGroup("Components"), SerializeField]
    private CharacterController _controller;

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
        
        _controller = this.GetOrAddComponent<CharacterController>();
        characterInput = GetComponent<BaseCharacterInput>();
        animator = this.GetOrAddComponent<Animator>();

        _controller.center = new Vector3(0, 0.5f, 0);
        _controller.height = 1;
        
        ApplySettings();
    }

    public override void DestroyDependencies()
    {
        base.DestroyDependencies();
        
        this.RemoveComponent<CharacterController>();
        this.RemoveComponent<BaseCharacterInput>();
        this.RemoveComponent<Animator>();
    }

    #endregion

    #region MonoBehaviours .........................................................................................

    protected override void OnAwake()
    {
        base.OnAwake();
        
        if (characterInput == null) Debug.LogError("Input Module must implement ICharacterInput");

        if (_cameraTransform == null && Camera.main != null) _cameraTransform = Camera.main.transform;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    
        // 1) Read raw 2D input (already normalized)
        Vector2 move2D = characterInput.ReadMovementInput();
        bool    run    = characterInput.IsRunning();
        bool    crouch = settings.canCrouch && characterInput.IsCrouching();
        bool    jump   = settings.canJump  && characterInput.JumpRequested();

        // Early-out if no camera
        if (_cameraTransform == null)
        {
            Debug.LogError("No cameraTransform assigned and Camera.main is null!");
            return;
        }

        // 2) Build camera-relative basis
        Vector3 camFwd = _cameraTransform.forward;
        camFwd.y = 0;
        camFwd.Normalize();

        Vector3 camRight = _cameraTransform.right;
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
                settings.rotationSpeed * Time.deltaTime
            );
        }

        // 5) Decide move speed
        float speed = crouch   ? settings.crouchSpeed
                    : run     ? settings.runSpeed
                              : settings.walkSpeed;

        // 6) Build final movement vector
        Vector3 move = desiredDir.normalized * speed;

        // 7) Handle jump + gravity
        if (_controller.isGrounded)
        {
            _verticalVelocity = -0.1f;
            if (jump)
                _verticalVelocity = settings.jumpForce;
        }
        _verticalVelocity += settings.gravity * Time.deltaTime;
        move.y = _verticalVelocity;

        // 8) Apply motion
        _controller.Move(move * Time.deltaTime);

        // 9) Smoothly adjust crouch height (optional)
        if (settings.canCrouch)
        {
            Debug.LogError("Crouch logic disabled, was messing with controller height. FIX");
            
            // float targetHeight = crouch ? settings.crouchHeight : settings.runSpeed;
            //_controller.height = Mathf.Lerp(_controller.height, targetHeight, 10f * Time.deltaTime);
        }
        

        // 10) Drive Animator parameters
        if (animator)
        {
            // TODO understand speed, vs motionspeed.
            
            animator.SetFloat(SpeedHash, move2D.magnitude == 0 ? 0 : speed);
            animator.SetBool (JumpHash,  jump);
            animator.SetBool (GroundedHash, _controller.isGrounded);
            bool freeFall = !_controller.isGrounded && _verticalVelocity < 0f;
            animator.SetBool (FreeFallHash, freeFall);
            // MotionSpeed can be full world‑space speed
            animator.SetFloat(MotionSpeedHash, move2D.magnitude);
        }
    }
    
    #endregion

    #region General ................................................................................................

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

    #region Event Handlers .........................................................................................

    #endregion

    #endregion
}
