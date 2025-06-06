using EmberAI.Attributes;
using EmberAI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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
        private string label;
        
        [FormerlySerializedAs("_headingPrefab")] [BoxGroup("Settings"), SerializeField]
        private Thumbnail HeadingPrefab;

        [FormerlySerializedAs("_thumbnailPrefab")] [BoxGroup("Settings"), SerializeField]
        private Thumbnail ThumbnailPrefab;

        [FormerlySerializedAs("_rectTransform")] [BoxGroup("Components"), SerializeField]
        private RectTransform rectTransform;
        
        [BoxGroup("Components"), SerializeField]
        private TextMeshProUGUI labelText;
        
        [BoxGroup("Components"), SerializeField] 
        private HorizontalOrVerticalLayoutGroup _contentLayout;
        
        [FormerlySerializedAs("_scrollRect")] [BoxGroup("Components"), SerializeField] 
        private ScrollRect ScrollRect;
        
        [FormerlySerializedAs("_contentSizeFitter")] [BoxGroup("Components"), Tooltip("Required for correct scrolling. Set H or V to 'preferred size"), SerializeField]
        private ContentSizeFitter ContentSizeFitter;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        public RectTransform RectTransform => rectTransform;
        
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

            rectTransform = this.GetComponent<RectTransform>();
            
            description = "Scrollable Thumbnail Selector UI";

            if (_contentLayout == null) _contentLayout = GetComponentInChildren<HorizontalOrVerticalLayoutGroup>();
            if(ScrollRect == null) ScrollRect = GetComponentInChildren<ScrollRect>();

            if (ScrollRect != null)
            {
                ScrollRect.movementType = ScrollRect.MovementType.Elastic;
            }

            if (_contentLayout != null)
            {
                ContentSizeFitter = _contentLayout.GetOrAddComponent<ContentSizeFitter>();

                if (_contentLayout is VerticalLayoutGroup)
                {
                    ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                }
                else
                {
                    ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                }
            }
        }

        #endregion

        #region MonoBehaviours .........................................................................................

        protected override void OnStart()
        {
            base.OnStart();
            
            if(labelText != null) labelText.text = label;
        }

        #endregion

        #region General ................................................................................................

        public void AddItem(string label, string imagePath)
        {
            Thumbnail instance = Instantiate(ThumbnailPrefab, _contentLayout.transform, true);
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