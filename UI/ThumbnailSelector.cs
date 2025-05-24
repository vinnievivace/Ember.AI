using EmberAI.Attributes;
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

        [BoxGroup("Settings"), SerializeField] 
        private HorizontalOrVerticalLayoutGroup _contentLayout;
        
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