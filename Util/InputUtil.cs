using System;
using UnityEngine;
using EmberAI.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace EmberAI.Core.Util
{
    public static class InputUtil
    {
        private static float _lastInputTime = -10f;
        private const float InputCooldownDuration = 0.35f;

        #if ENABLE_INPUT_SYSTEM
        private static bool UseNewInputSystem => (Keyboard.current != null) || (Gamepad.current != null) || (Mouse.current != null);
        #endif

        public static bool IsLeftMouseDown()
        {
            #if ENABLE_INPUT_SYSTEM
            if (UseNewInputSystem && Mouse.current != null) return Mouse.current.leftButton.isPressed;
            #endif
            return Input.GetMouseButton(0);
            
        }

        public static bool IsRightMouseDown()
        {
            #if ENABLE_INPUT_SYSTEM
            if (UseNewInputSystem && Mouse.current != null) return Mouse.current.rightButton.isPressed;
            #endif
            return Input.GetMouseButtonDown(1);
            
        }

        public static bool IsKeyDown(KeyCode keyCode)
        {
            #if ENABLE_INPUT_SYSTEM
            if (UseNewInputSystem && Keyboard.current != null)
            {
                if (Enum.TryParse<Key>(keyCode.ToString(), out var newKey))
                {
                    KeyControl keyControl = Keyboard.current[newKey];
                    if (keyControl != null) return keyControl.isPressed;
                }
            }
            #endif
            return Input.GetKey(keyCode);
            
        }

        public static bool WasKeyPressed(KeyCode keyCode)
        {
            #if ENABLE_INPUT_SYSTEM
            if (UseNewInputSystem && Keyboard.current != null)
            {
                if (Enum.TryParse<Key>(keyCode.ToString(), out var newKey))
                {
                    KeyControl keyControl = Keyboard.current[newKey];
                    if (keyControl != null) return keyControl.wasPressedThisFrame;
                }
            }
            #endif
            return Input.GetKeyDown(keyCode);
            
        }
        
        public static float GetAxis(string axisName)
        {
            #if ENABLE_INPUT_SYSTEM
            if (UseNewInputSystem)
            {
                switch (axisName)
                {
                    case "Horizontal":
                        return Keyboard.current != null
                            ? (Keyboard.current.aKey.isPressed ? -1f : 0f) +
                              (Keyboard.current.dKey.isPressed ? 1f : 0f)
                            : 0f;

                    case "Vertical":
                        return Keyboard.current != null
                            ? (Keyboard.current.sKey.isPressed ? -1f : 0f) +
                              (Keyboard.current.wKey.isPressed ? 1f : 0f)
                            : 0f;

                    case "Mouse X":
                        return Mouse.current?.delta.x.ReadValue() ?? 0f;

                    case "Mouse Y":
                        return Mouse.current?.delta.y.ReadValue() ?? 0f;

                    case "Mouse ScrollWheel":
                        return Mouse.current?.scroll.y.ReadValue() ?? 0f;

                    default:
                        Debug.LogWarning($"[InputUtil] Axis '{axisName}' not supported in Input System.");
                        return 0f;
                }
            }
            #endif
            return Input.GetAxis(axisName);
        }

        public static Vector2 GetMouseAxis()
        {
            return new Vector2(GetAxis("Mouse X"), GetAxis("Mouse Y"));
        }

        public static float GetMouseScroll()
        {
            return GetAxis("Mouse ScrollWheel");
        }

        public static bool AnyCurrentInput()
        {
            if (UIManager.Instance != null && UIManager.Instance.UIInteraction) return false;

            bool hasInput = false;

            #if ENABLE_INPUT_SYSTEM
            if (UseNewInputSystem)
            {
                Mouse mouse = Mouse.current;
                Keyboard keyboard = Keyboard.current;

                if (mouse != null)
                {
                    if ((mouse.leftButton?.isPressed ?? false) ||
                        (mouse.rightButton?.isPressed ?? false) ||
                        (mouse.middleButton?.isPressed ?? false) ||
                        mouse.scroll.ReadValue() != Vector2.zero)
                    {
                        hasInput = true;
                    }
                }

                if (!hasInput && keyboard != null)
                {
                    foreach (KeyControl key in keyboard.allKeys)
                    {
                        if (key?.isPressed ?? false)
                        {
                            hasInput = true;
                            break;
                        }
                    }
                }
            }
            #endif
            // Legacy fallback
            if (!hasInput &&
                (Input.anyKey ||
                 Input.GetMouseButton(0) ||
                 Input.GetMouseButton(1) ||
                 Input.GetMouseButton(2) ||
                 Input.mouseScrollDelta.y != 0))
            {
                hasInput = true;
            }

            // Update input timestamp
            if (hasInput)
            {
                _lastInputTime = Time.time;
            }

            return Time.time - _lastInputTime < InputCooldownDuration;
            
        }
    }
}
