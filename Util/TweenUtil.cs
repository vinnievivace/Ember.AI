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
    public static Coroutine TweenFloat(float startValue, float endValue, float durationInSeconds, Action<float> onTweenUpdate, Action onTweenComplete = null)
    {
        return CoroutineRunner.Instance.StartCoroutine(TweenFloatCoroutine(startValue, endValue, durationInSeconds, onTweenUpdate, onTweenComplete));
    }

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

    /// <summary>
    /// Tweens a Vector3 from startValue to endValue over a duration that’s
    /// scaled by the largest axis‐delta (so big moves take longer, small moves shorter).
    /// </summary>
    public static Coroutine TweenVector3(this MonoBehaviour owner, Vector3 startValue, Vector3 endValue, float baseDuration, Action<Vector3> onTweenUpdate, Action onTweenComplete = null)
    {
        // compute scale factor (clamped to [0,1] so duration stays sensible)
        float dx = Mathf.Abs(endValue.x - startValue.x);
        float dy = Mathf.Abs(endValue.y - startValue.y);
        float dz = Mathf.Abs(endValue.z - startValue.z);
        float maxDelta = Mathf.Max(dx, dy, dz);
        float ratio = Mathf.Min(maxDelta, 1f);
        float duration = baseDuration * ratio;

        return owner.StartCoroutine(TweenVector3Coroutine(startValue, endValue, duration, onTweenUpdate, onTweenComplete));
    }

    private static IEnumerator TweenVector3Coroutine(Vector3 start, Vector3 end, float duration, Action<Vector3> onUpdate, Action onComplete)
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
            Vector3 value = Vector3.Lerp(start, end, t);
            onUpdate?.Invoke(value);
            yield return null;
        }

        onUpdate?.Invoke(end);
        onComplete?.Invoke();
    }
        
    /*public static Tweener TweenFloat(float startValue, float endValue, float durationInSeconds, Action<float> onTweenFloatUpdate, Action onTweenFloatComplete = null)
    {
        float value = startValue;

        TweenerCore<float, float, FloatOptions> tween = DOTween.To(() => value, x => value = x, endValue, durationInSeconds);
    
        tween.onUpdate += delegate { onTweenFloatUpdate(value); };
    
        if(onTweenFloatComplete !=null) tween.onComplete += delegate { onTweenFloatComplete(); };

        return tween;

    }
    
    public static Tweener TweenVector3(Vector3 startValue, Vector3 endValue, float baseDuration, Action<Vector3> onTweenFloatUpdate, Action onTweenFloatComplete = null)
    {
        Vector3 value = startValue;

        // Calculate the magnitude of change for each axis
        float deltaX = Mathf.Abs(endValue.x - startValue.x);
        float deltaY = Mathf.Abs(endValue.y - startValue.y);
        float deltaZ = Mathf.Abs(endValue.z - startValue.z);
        float maxDelta = Mathf.Max(deltaX, deltaY, deltaZ);

        // Calculate the proportional duration
        // Assuming baseDuration is calibrated for a significant change
        float duration = baseDuration * (maxDelta / Mathf.Max(1f, maxDelta));  // Avoid division by zero, ensure at least 1

        TweenerCore<Vector3, Vector3, VectorOptions> tween = DOTween.To(() => value, 
            newValue => 
            {
                value = newValue; 
            }, endValue, duration);

        tween.onUpdate += () => onTweenFloatUpdate(value);

        if(onTweenFloatComplete !=null) tween.onComplete += delegate { onTweenFloatComplete(); };

        return tween;
    }

    /*public static Tweener TweenMaterialAlpha(Renderer target, string shaderColorProperty, float startValue, float endValue, float duration)
    {
        if (!target.material.HasProperty(shaderColorProperty))
        {
            Debug.LogError(target.material.name + " does not have a property named " + shaderColorProperty);

            return null;
        }
        Color startColor = target.material.GetColor(shaderColorProperty);
        Color tweenColor = target.material.GetColor(shaderColorProperty);

        if (!target.material.HasProperty("_SurfaceType"))
        {
            Debug.LogError(target.material.name + " does not have a property named _Surface, so cannot set to Transparent, and therefore cannot tween alpha");

            return null;
        }
        
        target.material.SetFloat("_Surface", 1.0f);
        
        startColor.a = startValue;
        
        return DOTween.To(() => startValue, x =>
        {
            Debug.LogWarning(tweenColor);
            
            tweenColor.a = x;
            target.material.SetColor(shaderColorProperty, tweenColor);
        }, endValue, duration);
    }*/
    }
}

