using EmberAI.Attributes;
using EmberAI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace EmberAI.UI
{
    public class ThumbnailSelector : UIBehaviour
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        public event System.Action<string> OnItemClicked;
        
        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        [BoxGroup("Settings"), SerializeField]
        private Thumbnail _headingPrefab, _thumbnailPrefab;

        [BoxGroup("Components"), SerializeField] 
        private HorizontalOrVerticalLayoutGroup _contentLayout;
        
        [BoxGroup("Components"), SerializeField] 
        private ScrollRect _scrollRect;
        
        [BoxGroup("Components"), Tooltip("Required for correct scrolling. Set H or V to 'preferred size"), SerializeField]
        private ContentSizeFitter _contentSizeFitter;
        
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

            description = "Scrollable Thumbnail Selector UI";

            if (_contentLayout == null) _contentLayout = GetComponentInChildren<HorizontalOrVerticalLayoutGroup>();
            if(_scrollRect == null) _scrollRect = GetComponentInChildren<ScrollRect>();

            if (_scrollRect != null)
            {
                _scrollRect.movementType = ScrollRect.MovementType.Elastic;
            }

            if (_contentLayout != null)
            {
                _contentSizeFitter = _contentLayout.GetOrAddComponent<ContentSizeFitter>();

                if (_contentLayout is VerticalLayoutGroup)
                {
                    _contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    _contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                }
                else
                {
                    _contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    _contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                }
            }
        }

        #endregion

        #region MonoBehaviours .........................................................................................

        #endregion

        #region General ................................................................................................

        public void AddItem(string label, string imagePath)
        {
            Thumbnail instance = Instantiate(_thumbnailPrefab, _contentLayout.transform, true);
            RectTransform layoutTransform = _contentLayout.GetComponent<RectTransform>();
            
            instance.GetComponent<RectTransform>().localScale = Vector3.one;
            
            instance.SetData(label, imagePath);;

            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutTransform);
            
        }
        
        public void DispatchItemClicked(Thumbnail thumbnail)
        {
            OnItemClicked?.Invoke(thumbnail.Label);
        }
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

#endregion
    }
}