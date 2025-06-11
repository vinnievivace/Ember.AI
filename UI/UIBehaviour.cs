
using EmberAI.Attributes;
using EmberAI.Core;
using UnityEngine;

namespace EmberAI.UI
{
    public abstract class UIBehaviour : EmberBehaviour
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        private bool _panelsVisible;
        private Tween _activeTween;
        
        [BoxGroup("Settings"), SerializeField]
        private Vector2 visiblePosition, hiddenPosition;

        [BoxGroup("Settings"), SerializeField, Tooltip("Delay in seconds before toggling UI visible when no input is detected.")] 
        private float UIToggleDuration = 0.25f;
        
        [BoxGroup("Components"), ReadOnly, SerializeField]
        private RectTransform rectTransform;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        #endregion

        #region Inspector ..............................................................................................

        #endregion

        #region Initialization .........................................................................................

        public override void InitializeDependencies()
        {
            base.InitializeDependencies();

            rectTransform = this.GetOrAddComponent<RectTransform>();
            
            if(visiblePosition == Vector2.zero) visiblePosition = rectTransform.anchoredPosition;
        }

        #endregion

        #region MonoBehaviours .........................................................................................

        #endregion

        #region General ................................................................................................

        public virtual void SetVisible(bool visible, bool tween = true)
        {
            if (_panelsVisible == visible) return;

            _panelsVisible = visible;

            SetPosition(visible ? visiblePosition : hiddenPosition, tween);
        }

        private void SetPosition(Vector2 position, bool tween = true)
        {
            if (tween)
            {
                _activeTween?.Cancel();

                _activeTween = TweenUtil.TweenVector2(rectTransform.anchoredPosition, position, UIToggleDuration, p => rectTransform.anchoredPosition = p, () => { });
            }
            else
            {
                rectTransform.anchoredPosition = position;
            }
        }
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

#endregion
    }
}