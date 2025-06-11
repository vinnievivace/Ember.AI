using System;
using System.Collections;
using Core;
using UnityEngine;

namespace EmberAI
{
    public class Tween
    {
        private Coroutine _coroutine;
        private MonoBehaviour _runner;

        public bool IsRunning => _coroutine != null;

        public Tween(MonoBehaviour runner, IEnumerator routine)
        {
            _runner = runner;
            _coroutine = _runner.StartCoroutine(Wrap(routine));
        }

        private IEnumerator Wrap(IEnumerator routine)
        {
            yield return routine;
            _coroutine = null;
        }

        public void Cancel()
        {
            if (_coroutine != null)
            {
                Debug.Log("canceling tween");
                
                _runner.StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }
    }
    
    public static class TweenUtil
    {
        public enum EaseType { Linear, EaseIn, EaseOut, EaseInOut }

        public static Tween TweenFloat(float startValue, float endValue, float durationInSeconds, Action<float> onTweenUpdate, Action onTweenComplete = null, EaseType ease = EaseType.Linear, MonoBehaviour runner = null)
        {
            return new Tween(runner ?? CoroutineRunner.Instance, TweenFloatCoroutine(startValue, endValue, durationInSeconds, onTweenUpdate, onTweenComplete, ease));
        }

        public static Tween TweenVector2(Vector2 startValue, Vector2 endValue, float baseDuration, Action<Vector2> onTweenUpdate, Action onTweenComplete = null, EaseType ease = EaseType.Linear, float maxDeltaClamp = 1f, MonoBehaviour runner = null)
        {
            float dx = Mathf.Abs(endValue.x - startValue.x);
            float dy = Mathf.Abs(endValue.y - startValue.y);
            float maxDelta = Mathf.Max(dx, dy);
            float ratio = Mathf.Clamp01(maxDelta / maxDeltaClamp);
            float duration = baseDuration * ratio;

            return new Tween(runner ?? CoroutineRunner.Instance, TweenVector2Coroutine(startValue, endValue, duration, onTweenUpdate, onTweenComplete, ease));
        }

        public static Tween TweenVector3(Vector3 startValue, Vector3 endValue, float duration, Action<Vector3> onTweenUpdate, Action onTweenComplete = null, EaseType ease = EaseType.Linear, MonoBehaviour runner = null)
        {
            return new Tween(runner ?? CoroutineRunner.Instance, TweenVector3Coroutine(startValue, endValue, duration, onTweenUpdate, onTweenComplete, ease));
        }

        public static Tween TweenColor(Color startValue, Color endValue, float duration, Action<Color> onTweenUpdate, Action onTweenComplete = null, EaseType ease = EaseType.Linear, MonoBehaviour runner = null)
        {
            return new Tween(runner ?? CoroutineRunner.Instance, TweenColorCoroutine(startValue, endValue, duration, onTweenUpdate, onTweenComplete, ease));
        }

        public static Tween TweenQuaternion(Quaternion startValue, Quaternion endValue, float duration, Action<Quaternion> onTweenUpdate, Action onTweenComplete = null, EaseType ease = EaseType.Linear, MonoBehaviour runner = null)
        {
            return new Tween(runner ?? CoroutineRunner.Instance, TweenQuaternionCoroutine(startValue, endValue, duration, onTweenUpdate, onTweenComplete, ease));
        }

        private static IEnumerator TweenFloatCoroutine(float start, float end, float duration, Action<float> onUpdate, Action onComplete, EaseType ease)
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
                float t = ApplyEase(Mathf.Clamp01(elapsed / duration), ease);
                onUpdate?.Invoke(Mathf.Lerp(start, end, t));
                yield return null;
            }

            onUpdate?.Invoke(end);
            onComplete?.Invoke();
        }

        private static IEnumerator TweenVector2Coroutine(Vector2 start, Vector2 end, float duration, Action<Vector2> onUpdate, Action onComplete, EaseType ease)
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
                float t = ApplyEase(Mathf.Clamp01(elapsed / duration), ease);
                onUpdate?.Invoke(Vector2.Lerp(start, end, t));
                yield return null;
            }

            onUpdate?.Invoke(end);
            onComplete?.Invoke();
        }

        private static IEnumerator TweenVector3Coroutine(Vector3 start, Vector3 end, float duration, Action<Vector3> onUpdate, Action onComplete, EaseType ease)
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
                float t = ApplyEase(Mathf.Clamp01(elapsed / duration), ease);
                onUpdate?.Invoke(Vector3.Lerp(start, end, t));
                yield return null;
            }

            onUpdate?.Invoke(end);
            onComplete?.Invoke();
        }

        private static IEnumerator TweenColorCoroutine(Color start, Color end, float duration, Action<Color> onUpdate, Action onComplete, EaseType ease)
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
                float t = ApplyEase(Mathf.Clamp01(elapsed / duration), ease);
                onUpdate?.Invoke(Color.Lerp(start, end, t));
                yield return null;
            }

            onUpdate?.Invoke(end);
            onComplete?.Invoke();
        }

        private static IEnumerator TweenQuaternionCoroutine(Quaternion start, Quaternion end, float duration, Action<Quaternion> onUpdate, Action onComplete, EaseType ease)
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
                float t = ApplyEase(Mathf.Clamp01(elapsed / duration), ease);
                onUpdate?.Invoke(Quaternion.Lerp(start, end, t));
                yield return null;
            }

            onUpdate?.Invoke(end);
            onComplete?.Invoke();
        }

        private static float ApplyEase(float t, EaseType ease)
        {
            return ease switch
            {
                EaseType.EaseIn     => t * t,
                EaseType.EaseOut    => 1f - Mathf.Pow(1f - t, 2),
                EaseType.EaseInOut  => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2) / 2f,
                _                   => t // Linear
            };
        }
    }
} 
