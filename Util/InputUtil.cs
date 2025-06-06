using System;
using EmberAI.UI;
using UnityEngine;
using UnityEngine.PlayerLoop;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace EmberAI.Core.Util
{
    public static class InputUtil
    {
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

        public static bool AnyCurrentInput()
        {
            if (UIManager.Instance != null && UIManager.Instance.UIInteraction) return false;

            #if ENABLE_INPUT_SYSTEM
            if (UseNewInputSystem)
            {
                Mouse mouse = Mouse.current;
                Keyboard keyboard = Keyboard.current;

                if (mouse != null)
                {
                    if ((mouse.leftButton?.isPressed ?? false) || 
                        (mouse.rightButton?.isPressed ?? false) || 
                        (mouse.middleButton?.isPressed ?? false)) 
                        return true;
                }

                if (keyboard != null)
                {
                    foreach (KeyControl key in keyboard.allKeys)
                    {
                        if (key?.isPressed ?? false) 
                            return true;
                    }
                }

                return false;
            }
            #endif

            // Legacy Input fallback: key or mouse buttons only, not movement
            if (Input.anyKey || Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2)) return true;

            return false;
        }
    }
}
