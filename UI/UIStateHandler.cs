using System;
using System.Runtime.InteropServices.WindowsRuntime;
using EmberAI.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EmberAI.UI
{
    public enum UIState { Default, Select, Drag, Hover, Click, Show, Hide }
    
    public class UIStateHandler : EmberBehaviour, ISelectHandler, IDeselectHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        public Action<UIStateHandler, UIState> OnStateChanged;
        
        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        // allowing inpector changes for debugging reasons!
        [BoxGroup("State"), SerializeField, OnValueChanged(nameof(OnStateChangedInInspector))]
        private UIState state;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        public UIState State => state;
        
        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        #endregion

        #region Inspector ..............................................................................................

        private void OnStateChangedInInspector()
        {
            OnStateChanged?.Invoke(this, state);
        }
        
        #endregion

        #region Initialization .........................................................................................

        public override void EditModeInitialize()
        {
            base.EditModeInitialize();
            
            description = "Broadcasts interactions to " + nameof(UIManager);
        }
        
        #endregion

        #region MonoBehaviours .........................................................................................

        #endregion

        #region General ................................................................................................

        /// <summary>
        /// Set State and dispatch <see cref="OnStateChanged"/>
        /// </summary>
        /// <param name="state"></param>
        public void SetState(UIState state)
        {
            if(this.state == state) return;
            
            this.state = state;
            
            OnStateChanged?.Invoke(this, state);
        }
        
        #endregion

        #region Event Handlers .........................................................................................

        public void OnSelect(BaseEventData eventData)
        {
            SetState(UIState.Select);
            
            UIManager.Instance.SetActiveStatus(this, true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            SetState(UIState.Default);
            
            UIManager.Instance.SetActiveStatus(this, false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            SetState(UIState.Drag);
            
            UIManager.Instance.SetActiveStatus(this, true);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            SetState(UIState.Default);
            
            UIManager.Instance.SetActiveStatus(this, false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetState(UIState.Hover);
            
            UIManager.Instance.SetActiveStatus(this, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetState(UIState.Default);
            
            UIManager.Instance.SetActiveStatus(this, false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SetState(UIState.Click);
            
            UIManager.Instance.SetActiveStatus(this, true);
        }
        
        #endregion

        #endregion
        
        }
}