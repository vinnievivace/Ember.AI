using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace EmberAI.Core.Util
{
    public static class InputUtil
    {
        // Runtime check for whether we should use the new Input System
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
        
        public static bool GetKey(KeyCode keyCode)
        {
            #if ENABLE_INPUT_SYSTEM
            if (UseNewInputSystem && Keyboard.current != null)
            {
                // try to map the legacy KeyCode name to the new InputSystem Key enum
                if (Enum.TryParse<Key>(keyCode.ToString(), out var newKey))
                {
                    KeyControl keyControl = Keyboard.current[newKey];
                    
                    if (keyControl != null) return keyControl.isPressed;
                }
            }
            #endif
            // fallback to legacy
            return Input.GetKey(keyCode);
        }
    }
}
