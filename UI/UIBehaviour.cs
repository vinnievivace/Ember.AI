
using System;
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

        private bool _panelsVisible, _tweenVisibility;
        private Tween _activeTween;
        
        [BoxGroup("Settings"), SerializeField]
        private Vector2 visiblePosition, hiddenPosition;

        [BoxGroup("Settings"), SerializeField, Tooltip("Delay in seconds before toggling UI visible when no input is detected.")] 
        private float UIToggleDuration = 0.25f;
        
        [BoxGroup("Components"), ReadOnly, SerializeField]
        private RectTransform rectTransform;
        
        [BoxGroup("Components"), ReadOnly, SerializeField]
        private UIStateHandler stateHandler;
        
        [BoxGroup("Audio"), SerializeField, Tooltip("when defined, these sounds override the defaults defined in the " + nameof(UISettings))] 
        private AudioClip customShowSound, customHideSound;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        public UIStateHandler StateHandler => stateHandler;
        
        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        #endregion

        #region Inspector ..............................................................................................

        #endregion

        #region Initialization .........................................................................................

        public override void EditModeInitialize()
        {
            base.EditModeInitialize();

            rectTransform = this.GetOrAddComponent<RectTransform>();
            stateHandler = this.GetOrAddComponent<UIStateHandler>();
            
            if(visiblePosition == Vector2.zero) visiblePosition = rectTransform.anchoredPosition;
        }

        #endregion

        #region MonoBehaviours .........................................................................................

        protected override void OnAwake()
        {
            base.OnAwake();
            
            stateHandler.OnStateChanged += OnStateChanged;
        }

        #endregion

        #region General ................................................................................................

        public virtual void SetVisible(bool visible, bool tween = true)
        {
            
            if (_panelsVisible == visible) return;
            
            _panelsVisible = visible;
            _tweenVisibility = tween;
            
            stateHandler.SetState(visible ? UIState.Show : UIState.Hide);
        }

        private void SetPosition(Vector2 position, bool tween = true)
        {
            if (tween)
            {
                _activeTween?.Cancel();

                _activeTween = TweenUtil.TweenVector2(rectTransform.anchoredPosition, position, UIToggleDuration, p => rectTransform.anchoredPosition = p,
                    () =>
                    {
                        stateHandler.SetState(UIState.Default);
                    });
            }
            else
            {
                rectTransform.anchoredPosition = position;
                
                stateHandler.SetState(UIState.Default);
            }
        }
        
        #endregion
        
        #region State Change Handlers ..................................................................................

        protected virtual void OnDefaultState() { }
        
        protected virtual void OnSelectState() { }
        
        protected virtual void OnDragState() { }

        protected virtual void OnShowState()
        {
            SetPosition(visiblePosition, _tweenVisibility);
            
            AudioClip showSound = !customShowSound ? UIManager.Instance.Settings.defaultShowSound : customShowSound;
            
            UIManager.Instance.PlayUISound(showSound);
        }

        protected virtual void OnHideState()
        {
            SetPosition(hiddenPosition, _tweenVisibility);
            
            AudioClip hideSound = !customHideSound ? UIManager.Instance.Settings.defaultHideSound : customHideSound;
            
            UIManager.Instance.PlayUISound(hideSound);
        }

        protected virtual void OnHoverState() { }

        protected virtual void OnClickState() { }
        
        #endregion

        #region Event Handlers .........................................................................................

        private void OnStateChanged(UIStateHandler handler, UIState state)
        {
            switch (state)
            {
                case UIState.Default: OnDefaultState(); break;
                case UIState.Select: OnSelectState(); break;
                case UIState.Drag: OnDragState(); break;
                case UIState.Hover: OnHoverState(); break;
                case UIState.Click: OnClickState(); break;
                case UIState.Show: OnShowState(); break;
                case UIState.Hide: OnHideState(); break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        
        #endregion

#endregion
    }
}