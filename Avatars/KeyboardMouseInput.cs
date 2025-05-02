using EmberAI.Avatars;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class KeyboardMouseInput : BaseCharacterInput
{
    [Header("Legacy Keys (only used if Legacy Input Manager is enabled)")]
    public KeyCode runKey    = KeyCode.LeftShift;
    public KeyCode jumpKey   = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftControl;

    #region Get Input Values ...........................................................................................
    
    private Vector2 ReadLegacyMovement()
    {
        float h = UnityEngine.Input.GetAxis("Horizontal");
        float v = UnityEngine.Input.GetAxis("Vertical");
        return new Vector2(h, v).normalized;
    }

    #if ENABLE_INPUT_SYSTEM
    private Vector2 ReadNewMovement()
    {
        var kb = Keyboard.current;
        if (kb == null) return Vector2.zero;

        float x = (kb.aKey.isPressed || kb.leftArrowKey.isPressed) ? -1f :
            (kb.dKey.isPressed || kb.rightArrowKey.isPressed) ? 1f : 0f;
        float y = (kb.wKey.isPressed || kb.upArrowKey.isPressed)    ? 1f :
            (kb.sKey.isPressed || kb.downArrowKey.isPressed)  ? -1f : 0f;
        return new Vector2(x, y).normalized;
    }
    #endif
    
    #if ENABLE_INPUT_SYSTEM
    private bool NewJump()
    {
        var kb = Keyboard.current;
        return kb != null && kb.spaceKey.wasPressedThisFrame;
    }
    #endif

    
#if ENABLE_INPUT_SYSTEM
    private bool NewRun()
    {
        var kb = Keyboard.current;
        return kb != null && kb.leftShiftKey.isPressed;
    }
#endif
    
#if ENABLE_INPUT_SYSTEM
    private bool NewCrouch()
    {
        var kb = Keyboard.current;
        return kb != null && kb.leftCtrlKey.isPressed;
    }
#endif
    
    #endregion
    
    public override Vector2 ReadMovementInput()
    {
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // New Input System only
        return ReadNewMovement();
        #elif !ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
        // Legacy only
        return ReadLegacyMovement();
        #elif ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
        // Both enabled – prefer New if available
        return (Keyboard.current != null) 
            ? ReadNewMovement() 
            : ReadLegacyMovement();
        #else
        return Vector2.zero;
        #endif
    }

    public override bool JumpRequested()
    {
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return NewJump();
        #elif !ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
        return UnityEngine.Input.GetKeyDown(jumpKey);
        #elif ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
        return (Keyboard.current != null) 
            ? NewJump() 
            : UnityEngine.Input.GetKeyDown(jumpKey);
        #else
        return false;
        #endif
    }

    public override bool IsRunning()
    {
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return NewRun();
        #elif !ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
        return UnityEngine.Input.GetKey(runKey);
        #elif ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
        return (Keyboard.current != null) 
            ? NewRun() 
            : UnityEngine.Input.GetKey(runKey);
        #else
        return false;
        #endif
    }

    public override bool IsCrouching()
    {
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return NewCrouch();
        #elif !ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
        return UnityEngine.Input.GetKey(crouchKey);
        #elif ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
        return (Keyboard.current != null) 
            ? NewCrouch() 
            : UnityEngine.Input.GetKey(crouchKey);
        #else
        return false;
        #endif
    }
}
