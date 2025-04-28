namespace EmberAI
{
    public static class TweenUtil
    {
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

