using UnityEngine;
using UnityEngine.EventSystems;

namespace EmberAI.UI
{
    public class UIInteractionDetector : EmberBehaviour,
        ISelectHandler, IDeselectHandler,
        IBeginDragHandler, IEndDragHandler,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerClickHandler
    {
        #region Initialization .........................................................................................

        public override void InitializeDependencies()
        {
            base.InitializeDependencies();
            description = "Broadcasts interactions to " + nameof(UIManager);
        }

        #endregion

        #region Event Handlers .........................................................................................

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

        public void OnPointerEnter(PointerEventData eventData)
        {
            UIManager.Instance.SetActiveStatus(this, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UIManager.Instance.SetActiveStatus(this, false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            UIManager.Instance.SetActiveStatus(this, true);
        }

        #endregion
    }
}