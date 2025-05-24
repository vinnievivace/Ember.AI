using System;
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
        private RawImage _image;
        
        [BoxGroup("Components"), SerializeField]
        private Button _button;

        [BoxGroup("Components"), SerializeField]
        private TextMeshProUGUI _labelTXT;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        public string Label => _labelTXT.text;
        
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
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        #endregion

        #region General ................................................................................................

        public async void SetData(string label, string imagePath)
        {
            _labelTXT.text = label;
            _image.texture = await TextureUtil.LoadFromURL(imagePath);
        }
        
        #endregion

        #region Event Handlers .........................................................................................

        private void OnClicked()
        {
            GetComponentInParent<ThumbnailSelector>().DispatchItemClicked(this);
        }
        
        #endregion

#endregion
    }
}