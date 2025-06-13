using System.Collections.Generic;
using Core;
using EmberAI.Attributes;
using EmberAI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmberAI.UI
{
    public class UIManager : EmberSingleton<UIManager>
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly List<UIStateHandler> _activeUI = new List<UIStateHandler>();

        [BoxGroup("Settings"), SerializeField]
        private UISettings settings;
        
        [BoxGroup("Components"), SerializeField]
        private AudioSource audioSource;
        
        [BoxGroup("State"), ReadOnly] 
        public bool UIInteraction;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        public UISettings Settings => settings;
        
        public ModalPopup ModalPopup => FindFirstObjectByType<ModalPopup>(FindObjectsInactive.Include);
        
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
            
            description = "Singleton for managing UI functionality";
            
            if(audioSource == null) audioSource = this.GetOrAddComponent<AudioSource>();
        }

        #endregion

        #region MonoBehaviours .........................................................................................

        protected override void OnStart()
        {
            base.OnStart();
            
            DiscoverUIComponents();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            
            UIInteraction = _activeUI.Count > 0;
        }

        #endregion

        #region General ................................................................................................

        /// <summary>
        /// Discovers UI Components that do not inherit from <see cref="UIBehaviour"/> but still need to be handled by the <see cref="UIStateHandler"/>
        /// </summary>
        public void DiscoverUIComponents()
        {
            foreach (TMP_InputField inputField in FindObjectsByType<TMP_InputField>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                inputField.GetOrAddComponent<UIStateHandler>();
            }
            foreach (ScrollRect scrollRect in FindObjectsByType<ScrollRect>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                scrollRect.GetOrAddComponent<UIStateHandler>();
            }
        }

        public void PlayUISound(AudioClip clip)
        {
            if(clip == null) return;
            
            audioSource.PlayOneShot(clip);
        }

        public void PlayUISound(UIBehaviour source)
        {
            switch (source.StateHandler.State)
            {
                
            }
        }

        public void SetActiveStatus(UIStateHandler detector, bool value)
        {
            if (value) _activeUI.AddIfNotFound(detector);
            else _activeUI.RemoveIfFound(detector);
        }
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

#endregion
    }
}