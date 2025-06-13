using System.Collections.Generic;
using EmberAI.Attributes;
using EmberAI.Attributes.EmberAI.Attributes;
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

        [BoxGroup("Audio"), Tooltip("when defined, these sounds override the defaults defined in the " + nameof(UISettings))] 
        public AudioClip customHoverSound, customClickSound;

        [BoxGroup("Components"), SerializeField]
        private TextMeshProUGUI labelText;
        
        [BoxGroup("Components"), SerializeField] 
        private HorizontalOrVerticalLayoutGroup ContentLayout;
        
        [BoxGroup("Components"), SerializeField] 
        private ScrollRect ScrollRect;
        
        [BoxGroup("Components"), Tooltip("Required for correct scrolling. Set H or V to 'preferred size"), SerializeField]
        private ContentSizeFitter ContentSizeFitter;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        #endregion

        #region Inspector ..............................................................................................

        [ButtonGroup("Initialize", "Set Default Layout", "Applies default layout settings based on Horizontal or Vertical Layout Group")]
        private void DoSetDefaultLayout()
        {
            SetDefaultLayout();
        }
        
        #endregion

        #region Initialization .........................................................................................

        public override void EditModeInitialize()
        {
            base.EditModeInitialize();

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
        
        private void SetDefaultLayout()
        {
            if (ContentLayout == null)
            {
                Debug.LogError("ThumbnailSelector: no ContentLayout assigned!");
                return;
            }

            // 1) Grab the RectTransform on the content container
            var rt = ContentLayout.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta        = Vector2.zero;

            bool isVertical = ContentLayout is VerticalLayoutGroup;
            bool isHorizontal = ContentLayout is HorizontalLayoutGroup;

            // 2) Anchors & pivot
            if (isVertical)
            {
                // stretch full width, pin top
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot     = new Vector2(0.5f, 1f);
            }
            else if (isHorizontal)
            {
                // stretch full height, pin left
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot     = new Vector2(0f, 0.5f);
            }

            // 3) LayoutGroup settings
            if (isVertical)
            {
                var v = (VerticalLayoutGroup)ContentLayout;
                v.padding              = new RectOffset(0, 0, 3, 5);
                v.spacing              = 5;
                v.childAlignment       = TextAnchor.UpperCenter;
                v.childControlWidth    = true;
                v.childControlHeight   = false;
                v.childForceExpandWidth  = true;
                v.childForceExpandHeight = false;
            }
            else if (isHorizontal)
            {
                var h = (HorizontalLayoutGroup)ContentLayout;
                h.padding              = new RectOffset(3, 5, 0, 0);
                h.spacing              = 5;
                h.childAlignment       = TextAnchor.MiddleLeft;
                h.childControlWidth    = false;
                h.childControlHeight   = true;
                h.childForceExpandWidth  = false;
                h.childForceExpandHeight = true;
            }

            // 4) ContentSizeFitter settings (make sure we have one)
            if (ContentSizeFitter == null)
                ContentSizeFitter = ContentLayout.GetOrAddComponent<ContentSizeFitter>();

            if (isVertical)
            {
                ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                ContentSizeFitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;
            }
            else if (isHorizontal)
            {
                ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                ContentSizeFitter.verticalFit   = ContentSizeFitter.FitMode.Unconstrained;
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