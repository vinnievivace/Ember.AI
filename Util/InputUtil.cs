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
                var keyControl = GetKeyControl(keyCode);
                if (keyControl != null) return keyControl.isPressed;
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
                var keyControl = GetKeyControl(keyCode);
                if (keyControl != null) return keyControl.wasPressedThisFrame;
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
        
        #if ENABLE_INPUT_SYSTEM
        private static KeyControl GetKeyControl(KeyCode keyCode)
        {
            if (Keyboard.current == null) return null;

            switch (keyCode)
            {
                // Standard letters
                case KeyCode.A: return Keyboard.current.aKey;
                case KeyCode.B: return Keyboard.current.bKey;
                case KeyCode.C: return Keyboard.current.cKey;
                case KeyCode.D: return Keyboard.current.dKey;
                case KeyCode.E: return Keyboard.current.eKey;
                case KeyCode.F: return Keyboard.current.fKey;
                case KeyCode.G: return Keyboard.current.gKey;
                case KeyCode.H: return Keyboard.current.hKey;
                case KeyCode.I: return Keyboard.current.iKey;
                case KeyCode.J: return Keyboard.current.jKey;
                case KeyCode.K: return Keyboard.current.kKey;
                case KeyCode.L: return Keyboard.current.lKey;
                case KeyCode.M: return Keyboard.current.mKey;
                case KeyCode.N: return Keyboard.current.nKey;
                case KeyCode.O: return Keyboard.current.oKey;
                case KeyCode.P: return Keyboard.current.pKey;
                case KeyCode.Q: return Keyboard.current.qKey;
                case KeyCode.R: return Keyboard.current.rKey;
                case KeyCode.S: return Keyboard.current.sKey;
                case KeyCode.T: return Keyboard.current.tKey;
                case KeyCode.U: return Keyboard.current.uKey;
                case KeyCode.V: return Keyboard.current.vKey;
                case KeyCode.W: return Keyboard.current.wKey;
                case KeyCode.X: return Keyboard.current.xKey;
                case KeyCode.Y: return Keyboard.current.yKey;
                case KeyCode.Z: return Keyboard.current.zKey;

                // Arrows
                case KeyCode.UpArrow: return Keyboard.current.upArrowKey;
                case KeyCode.DownArrow: return Keyboard.current.downArrowKey;
                case KeyCode.LeftArrow: return Keyboard.current.leftArrowKey;
                case KeyCode.RightArrow: return Keyboard.current.rightArrowKey;

                // Control keys
                case KeyCode.Space: return Keyboard.current.spaceKey;
                case KeyCode.LeftShift: return Keyboard.current.leftShiftKey;
                case KeyCode.RightShift: return Keyboard.current.rightShiftKey;
                case KeyCode.LeftControl: return Keyboard.current.leftCtrlKey;
                case KeyCode.RightControl: return Keyboard.current.rightCtrlKey;
                case KeyCode.Tab: return Keyboard.current.tabKey;
                case KeyCode.Escape: return Keyboard.current.escapeKey;
                case KeyCode.Return: return Keyboard.current.enterKey;
                case KeyCode.Backspace: return Keyboard.current.backspaceKey;

                // Top row numbers
                case KeyCode.Alpha0: return Keyboard.current.digit0Key;
                case KeyCode.Alpha1: return Keyboard.current.digit1Key;
                case KeyCode.Alpha2: return Keyboard.current.digit2Key;
                case KeyCode.Alpha3: return Keyboard.current.digit3Key;
                case KeyCode.Alpha4: return Keyboard.current.digit4Key;
                case KeyCode.Alpha5: return Keyboard.current.digit5Key;
                case KeyCode.Alpha6: return Keyboard.current.digit6Key;
                case KeyCode.Alpha7: return Keyboard.current.digit7Key;
                case KeyCode.Alpha8: return Keyboard.current.digit8Key;
                case KeyCode.Alpha9: return Keyboard.current.digit9Key;

                // Numpad numbers
                case KeyCode.Keypad0: return Keyboard.current.numpad0Key;
                case KeyCode.Keypad1: return Keyboard.current.numpad1Key;
                case KeyCode.Keypad2: return Keyboard.current.numpad2Key;
                case KeyCode.Keypad3: return Keyboard.current.numpad3Key;
                case KeyCode.Keypad4: return Keyboard.current.numpad4Key;
                case KeyCode.Keypad5: return Keyboard.current.numpad5Key;
                case KeyCode.Keypad6: return Keyboard.current.numpad6Key;
                case KeyCode.Keypad7: return Keyboard.current.numpad7Key;
                case KeyCode.Keypad8: return Keyboard.current.numpad8Key;
                case KeyCode.Keypad9: return Keyboard.current.numpad9Key;

                // Numpad operators
                case KeyCode.KeypadEnter: return Keyboard.current.numpadEnterKey;
                case KeyCode.KeypadPeriod: return Keyboard.current.numpadPeriodKey;
                case KeyCode.KeypadPlus: return Keyboard.current.numpadPlusKey;
                case KeyCode.KeypadMinus: return Keyboard.current.numpadMinusKey;
                case KeyCode.KeypadMultiply: return Keyboard.current.numpadMultiplyKey;
                case KeyCode.KeypadDivide: return Keyboard.current.numpadDivideKey;
                case KeyCode.KeypadEquals: return Keyboard.current.numpadEqualsKey;

                default:
                    Debug.LogError($"[InputUtil] Unsupported KeyCode '{keyCode}' in new Input System.");
                    return null;
            }
        }
        #endif

    }
}
