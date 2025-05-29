using EmberAI.Attributes;
using EmberAI.Avatars;
using EmberAI.Core.Util;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Handles keyboard and mouse input, supporting both the legacy Input Manager
/// and the new Input System with a single unified interface.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class KeyboardMouseInput : BaseCharacterInput
{
    #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

    #endregion

    #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

    #endregion

    #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

    [BoxGroup("Settings")]
    public KeyCode runKey = KeyCode.LeftShift, jumpKey = KeyCode.Space, crouchKey = KeyCode.LeftControl;
    
    [BoxGroup("State"), SerializeField, ReadOnly] 
    private bool usingLegacyInput;
    
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
        
        #if ENABLE_INPUT_SYSTEM
        usingLegacyInput = false;
        #else
        usingLegacyInput = true;
        #endif
    }

    #endregion

    #region MonoBehaviours .........................................................................................

    #endregion

    #region General ................................................................................................

    public override Vector2 ReadMovementInput()
    {
        #if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        
        if (kb == null) return Vector2.zero;

        float x = (kb.aKey.isPressed || kb.leftArrowKey.isPressed) ? -1f : (kb.dKey.isPressed || kb.rightArrowKey.isPressed) ?  1f : 0f;
        float y = (kb.wKey.isPressed || kb.upArrowKey.isPressed)    ?  1f : (kb.sKey.isPressed || kb.downArrowKey.isPressed)  ? -1f : 0f;
        
        return new Vector2(x, y).normalized;
        #else
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        return new Vector2(h, v).normalized;
        #endif
    }

    public override bool JumpRequested()
    {
        return InputUtil.IsKeyDown(jumpKey);
    }

    public override bool IsRunning()
    {
        return InputUtil.IsKeyDown(runKey);

        #if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        return kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed);
        #else
        return Input.GetKey(runKey);
        #endif
    }

    public override bool IsCrouching()
    {
        #if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        return kb != null && (kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed);
        #else
        return Input.GetKey(crouchKey);
        #endif
    }
    
    #endregion

    #region Event Handlers .........................................................................................

    #endregion

    #endregion
    
}
