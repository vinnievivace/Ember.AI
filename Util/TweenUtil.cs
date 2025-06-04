using System;
using System.Collections;
using Core;
using UnityEngine;

namespace EmberAI
{
    public static class TweenUtil
    {
        /// <summary>
        /// Tweens a float from startValue to endValue over duration seconds.
        /// Returns the Coroutine so you can StopCoroutine if needed.
        /// </summary>
        public static void TweenFloat(float startValue, float endValue, float durationInSeconds, Action<float> onTweenUpdate, Action onTweenComplete = null)
        {
            CoroutineRunner.Instance.StartCoroutine(TweenFloatCoroutine(startValue, endValue, durationInSeconds, onTweenUpdate, onTweenComplete));
        }
        
        /// <summary>
        /// Tweens a Vector2 from startValue to endValue over a duration that’s
        /// scaled by the largest axis‐delta (so big moves take longer, small moves shorter).
        /// </summary>
        public static void TweenVector2(Vector2 startValue, Vector2 endValue, float baseDuration, Action<Vector2> onTweenUpdate, Action onTweenComplete = null)
        {
            // compute scale factor (clamped to [0,1] so duration stays sensible)
            float dx = Mathf.Abs(endValue.x - startValue.x);
            float dy = Mathf.Abs(endValue.y - startValue.y);
            float maxDelta = Mathf.Max(dx, dy);
            float ratio = Mathf.Min(maxDelta, 1f);
            float duration = baseDuration * ratio;

            CoroutineRunner.Instance.StartCoroutine(TweenVector2Coroutine(startValue, endValue, duration, onTweenUpdate, onTweenComplete));
        }
        
        #region Coroutines .............................................................................................
        
        private static IEnumerator TweenFloatCoroutine(float start, float end, float duration, Action<float> onUpdate, Action onComplete)
        {
            if (duration <= 0f)
            {
                onUpdate?.Invoke(end);
                onComplete?.Invoke();
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float value = Mathf.Lerp(start, end, t);
                onUpdate?.Invoke(value);
                yield return null;
            }

            // ensure final value & completion
            onUpdate?.Invoke(end);
            onComplete?.Invoke();
        }

        private static IEnumerator TweenVector2Coroutine(Vector2 start, Vector2 end, float duration, Action<Vector2> onUpdate, Action onComplete)
        {
            if (duration <= 0f)
            {
                onUpdate?.Invoke(end);
                onComplete?.Invoke();
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                Vector2 value = Vector2.Lerp(start, end, t);
                onUpdate?.Invoke(value);
                yield return null;
            }

            onUpdate?.Invoke(end);
            onComplete?.Invoke();
        }
        
        #endregion
        
        

        
    }
}
