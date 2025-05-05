using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EmberAI.Core.Util
{
    public static class InputUtil
{
    // Runtime check for whether we should use the new Input System
#if ENABLE_INPUT_SYSTEM
    private static bool UseNewInputSystem =>
        (Keyboard.current != null) ||
        (Gamepad.current != null) ||
        (Mouse.current != null);
#endif

    public static float GetHorizontal()
    {
#if ENABLE_INPUT_SYSTEM
        if (UseNewInputSystem)
        {
            float value = 0f;

            // Gamepad stick
            if (Gamepad.current != null)
                value = Gamepad.current.leftStick.ReadValue().x;

            // Keyboard A/D or Left/Right
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)  value -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) value += 1f;
            }

            return Mathf.Clamp(value, -1f, 1f);
        }
        else
#endif
        {
            // Legacy axis
            return Input.GetAxis("Horizontal");
        }
    }

    public static float GetVertical()
    {
#if ENABLE_INPUT_SYSTEM
        if (UseNewInputSystem)
        {
            float value = 0f;

            // Gamepad stick
            if (Gamepad.current != null)
                value = Gamepad.current.leftStick.ReadValue().y;

            // Keyboard W/S or Up/Down
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)   value += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) value -= 1f;
            }

            return Mathf.Clamp(value, -1f, 1f);
        }
        else
#endif
        {
            // Legacy axis
            return Input.GetAxis("Vertical");
        }
    }

    public static float GetMouseX()
    {
#if ENABLE_INPUT_SYSTEM
        if (UseNewInputSystem && Mouse.current != null)
            return Mouse.current.delta.ReadValue().x;
        else
#endif
            return Input.GetAxis("Mouse X");
    }

    public static float GetMouseY()
    {
#if ENABLE_INPUT_SYSTEM
        if (UseNewInputSystem && Mouse.current != null)
            return Mouse.current.delta.ReadValue().y;
        else
#endif
            return Input.GetAxis("Mouse Y");
    }

    public static bool IsLeftMouseDown()
    {
#if ENABLE_INPUT_SYSTEM
        if (UseNewInputSystem && Mouse.current != null)
            return Mouse.current.leftButton.wasPressedThisFrame;
        else
#endif
            return Input.GetMouseButtonDown(0);
    }

    public static bool IsRightMouseDown()
    {
#if ENABLE_INPUT_SYSTEM
        if (UseNewInputSystem && Mouse.current != null)
            return Mouse.current.rightButton.wasPressedThisFrame;
        else
#endif
            return Input.GetMouseButtonDown(1);
    }
    
    public static Vector2 GetMouseAxis()
    {
#if ENABLE_INPUT_SYSTEM
        if (UseNewInputSystem && Mouse.current != null)
            // new Input System: raw delta
            return Mouse.current.delta.ReadValue();
        else
#endif
            // legacy Input Manager
            return new Vector2(
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Mouse Y")
            );
    }
    
    public static bool GetKey(KeyCode keyCode)
    {
#if ENABLE_INPUT_SYSTEM
        if (UseNewInputSystem && Keyboard.current != null)
        {
            // try to map the legacy KeyCode name to the new InputSystem Key enum
            if (Enum.TryParse<UnityEngine.InputSystem.Key>(keyCode.ToString(), out var newKey))
            {
                var keyControl = Keyboard.current[newKey];
                if (keyControl != null)
                    return keyControl.isPressed;
            }
        }
#endif
        // fallback to legacy
        return Input.GetKey(keyCode);
    }

}

}
