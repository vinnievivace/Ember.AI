using EmberAI.Attributes;
using EmberAI.Core.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmberAI.UI
{
    public class Thumbnail : UIBehaviour
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        [BoxGroup("Components"), SerializeField]
        private RawImage Image;
        
        [BoxGroup("Components"), SerializeField]
        private Button Button;

        [BoxGroup("Components"), SerializeField]
        private TextMeshProUGUI LabelTXT;
        
        [BoxGroup("Components"), SerializeField]
        private Image SelectedIndicator;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        public string Label => LabelTXT.text;
        
        public bool IsSelected { get; set; }

        private ThumbnailSelector Selector => GetComponentInParent<ThumbnailSelector>();
        
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
            
            description = "Thumbnail for rendering items inside a " + nameof(ThumbnailSelector);
            
        }

        #endregion

        #region MonoBehaviours .........................................................................................

        private void OnEnable()
        {
            Button.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            Button.onClick.RemoveListener(OnClicked);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            
            SelectedIndicator.gameObject.SetActive(IsSelected);
        }

        #endregion

        #region General ................................................................................................

        public async void SetData(string label, string imagePath)
        {
            LabelTXT.text = label;
            Image.texture = await TextureUtil.LoadFromURL(imagePath);
        }

        public void SetSelected(bool value)
        {
            if(IsSelected == value) return;
            
            IsSelected = value;
            
        }

        #endregion
        
        #region State Change Handlers ..................................................................................

        protected override void OnHoverState()
        {
            base.OnHoverState();
            
            AudioClip sound = Selector.customHoverSound == null ? UIManager.Instance.Settings.defaultHoverSound : Selector.customHoverSound;
            
            UIManager.Instance.PlayUISound(sound);
        }

        protected override void OnClickState()
        {
            base.OnClickState();
            
            AudioClip sound = Selector.customClickSound == null ? UIManager.Instance.Settings.defaultClickSound : Selector.customClickSound;
            
            UIManager.Instance.PlayUISound(sound);
        }
        
        #endregion

        #region Event Handlers .........................................................................................

        private void OnClicked()
        {
            if (IsSelected) return;
            
            Selector.DispatchItemClicked(this);
            
            // yuck. ok so i will regret this, but for now, the Button click changes the state..
            StateHandler.SetState(UIState.Click);
        }
        
        #endregion

        #endregion
    }
}