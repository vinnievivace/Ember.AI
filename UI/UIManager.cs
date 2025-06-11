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

        private List<UIInteractionDetector> _activeUI = new List<UIInteractionDetector>();

        [BoxGroup("State"), ReadOnly] 
        public bool UIInteraction;
        
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
            
            description = "Singleton for managing UI functionality";
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

        public void DiscoverUIComponents()
        {
            foreach (UIBehaviour uiBehaviour in FindObjectsByType<UIBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                uiBehaviour.GetOrAddComponent<UIInteractionDetector>();
            }
            foreach (TMP_InputField inputField in FindObjectsByType<TMP_InputField>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                inputField.GetOrAddComponent<UIInteractionDetector>();
            }
            foreach (ScrollRect scrollRect in FindObjectsByType<ScrollRect>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                scrollRect.GetOrAddComponent<UIInteractionDetector>();
            }
        }

        public void SetActiveStatus(UIInteractionDetector detector, bool value)
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