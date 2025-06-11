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

        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        private static bool IsUsingNewInputSystem => true;
        #elif !ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
        private static bool IsUsingNewInputSystem => false;
        #else
        private static bool IsUsingNewInputSystem => (Keyboard.current != null) || (Gamepad.current != null) || (Mouse.current != null);
        #endif

        public static bool IsLeftMouseDown()
        {
            #if ENABLE_INPUT_SYSTEM
            if (IsUsingNewInputSystem && Mouse.current != null) return Mouse.current.leftButton.isPressed;
            #endif
            #if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButton(0);
            #else
            return false;
            #endif
        }

        public static bool IsRightMouseDown()
        {
            #if ENABLE_INPUT_SYSTEM
            if (IsUsingNewInputSystem && Mouse.current != null) return Mouse.current.rightButton.isPressed;
            #endif
            #if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(1);
            #else
            return false;
            #endif
        }

        public static bool IsKeyDown(KeyCode keyCode)
        {
            #if ENABLE_INPUT_SYSTEM
            if (IsUsingNewInputSystem && Keyboard.current != null)
            {
                if (Enum.TryParse<Key>(keyCode.ToString(), out var newKey))
                {
                    KeyControl keyControl = Keyboard.current[newKey];
                    if (keyControl != null) return keyControl.isPressed;
                }
            }
            #endif
            #if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(keyCode);
            #else
            return false;
            #endif
        }

        public static bool WasKeyPressed(KeyCode keyCode)
        {
            #if ENABLE_INPUT_SYSTEM
            if (IsUsingNewInputSystem && Keyboard.current != null)
            {
                if (Enum.TryParse<Key>(keyCode.ToString(), out var newKey))
                {
                    KeyControl keyControl = Keyboard.current[newKey];
                    if (keyControl != null) return keyControl.wasPressedThisFrame;
                }
            }
            #endif
            #if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(keyCode);
            #else
            return false;
            #endif
        }

        public static float GetAxis(string axisName)
        {
            #if ENABLE_INPUT_SYSTEM
            if (IsUsingNewInputSystem)
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
            #if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetAxis(axisName);
            #else
            return 0f;
            #endif
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
            if (IsUsingNewInputSystem)
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
            #if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
            if (!hasInput &&
                (Input.anyKey ||
                 Input.GetMouseButton(0) ||
                 Input.GetMouseButton(1) ||
                 Input.GetMouseButton(2) ||
                 Input.mouseScrollDelta.y != 0))
            {
                hasInput = true;
            }
            #endif

            if (hasInput)
            {
                _lastInputTime = Time.time;
            }

            return Time.time - _lastInputTime < InputCooldownDuration;
        }
    }
}
