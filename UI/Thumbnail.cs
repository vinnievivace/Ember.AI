using System;
using EmberAI.Attributes;
using EmberAI.Core.Util;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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

        #region Event Handlers .........................................................................................

        private void OnClicked()
        {
            if (IsSelected) return;
            
            GetComponentInParent<ThumbnailSelector>().DispatchItemClicked(this);
        }
        
        #endregion

#endregion
    }
}