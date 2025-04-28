using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;
using MouseButton = UnityEngine.InputSystem.LowLevel.MouseButton;

namespace EmberAI.Util
{
    public static class InputUtil
    {
        private static float _lastClickTime = -1f;
        private const float DoubleClickMax = 0.5f; // 500 milliseconds

        public static bool DidDoubleClick(MouseButton button = MouseButton.Left)
        {
            if (Input.GetMouseButtonDown((int)button))
            {
                float currentTime = Time.time;

                if (_lastClickTime >= 0 && (currentTime - _lastClickTime) <= DoubleClickMax)
                {
                    // Double-click detected
                    _lastClickTime = -1f; // Reset after a successful double-click
                    return true;
                }
                else
                {
                    // Record the time of the first click
                    _lastClickTime = currentTime;
                }
            }

            return false;
        }

        public static bool DidClickObject(GameObject target, MouseButton button = MouseButton.Left, bool doubleClick = false)
        {
            if (!Input.GetMouseButtonDown((int)button)) return false;
            if (target == null) return false;
            if (doubleClick && !DidDoubleClick(button)) return false;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                if (hit.transform == target.transform) return true;
            }

            return false;
        }
    }
}