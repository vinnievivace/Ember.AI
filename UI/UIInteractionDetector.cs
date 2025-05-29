using UnityEngine.EventSystems;

namespace EmberAI.UI
{
    public class UIInteractionDetector : EmberBehaviour, ISelectHandler, IDeselectHandler, IBeginDragHandler, IEndDragHandler
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

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

            description = "Broadcasts interactions to " + nameof(UIManager);
        }

        #endregion

        #region MonoBehaviours .........................................................................................
       
        #endregion

        #region General ................................................................................................

        public void OnSelect(BaseEventData eventData)
        {
            UIManager.Instance.SetActiveStatus(this, true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            UIManager.Instance.SetActiveStatus(this, false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            UIManager.Instance.SetActiveStatus(this, true);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            UIManager.Instance.SetActiveStatus(this, false);
        }
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

#endregion


    }
}