using System.Collections.Generic;
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

        private readonly List<Thumbnail> _thumbnails = new List<Thumbnail>();

        private Thumbnail _selectedThumbnail;
        
        [BoxGroup("Settings"), SerializeField] 
        private string label;
        
        [BoxGroup("Settings"), SerializeField]
        private Thumbnail HeadingPrefab;

        [BoxGroup("Settings"), SerializeField]
        private Thumbnail ThumbnailPrefab;

        [BoxGroup("Components"), SerializeField]
        private TextMeshProUGUI labelText;
        
        [FormerlySerializedAs("_contentLayout")] [BoxGroup("Components"), SerializeField] 
        private HorizontalOrVerticalLayoutGroup ContentLayout;
        
        [FormerlySerializedAs("_scrollRect")] [BoxGroup("Components"), SerializeField] 
        private ScrollRect ScrollRect;
        
        [FormerlySerializedAs("_contentSizeFitter")] [BoxGroup("Components"), Tooltip("Required for correct scrolling. Set H or V to 'preferred size"), SerializeField]
        private ContentSizeFitter ContentSizeFitter;
        
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

            if (ContentLayout == null) ContentLayout = GetComponentInChildren<HorizontalOrVerticalLayoutGroup>();
            if(ScrollRect == null) ScrollRect = GetComponentInChildren<ScrollRect>();

            if (ScrollRect != null)
            {
                ScrollRect.movementType = ScrollRect.MovementType.Elastic;
            }

            if (ContentLayout != null)
            {
                ContentSizeFitter = ContentLayout.GetOrAddComponent<ContentSizeFitter>();

                if (ContentLayout is VerticalLayoutGroup)
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
            Thumbnail instance = Instantiate(ThumbnailPrefab, ContentLayout.transform, true);
            RectTransform layoutTransform = ContentLayout.GetComponent<RectTransform>();
            
            instance.GetComponent<RectTransform>().localScale = Vector3.one;
            
            instance.SetData(label, imagePath);

            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutTransform);
            
            _thumbnails.Add(instance);
        }

        public void RemoveAllItems()
        {
            foreach (Thumbnail thumbnail in _thumbnails)
            {
                Destroy(thumbnail.gameObject);
            }
            _thumbnails.Clear();
            
            RectTransform layoutTransform = ContentLayout.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutTransform);
        }
        
        public void DispatchItemClicked(Thumbnail thumbnail)
        {
            OnItemClicked?.Invoke(thumbnail.Label);
            
            if(_selectedThumbnail != null) _selectedThumbnail.SetSelected(false);
            
            _selectedThumbnail = thumbnail;
            _selectedThumbnail.SetSelected(true);
        }
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

        #endregion
    }
}