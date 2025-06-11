using System;
using EmberAI.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EmberAI.UI
{
    public class ModalPopup : UIBehaviour
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        [BoxGroup("Components"), SerializeField]
        private TextMeshProUGUI headingText, messageText;
        
        [BoxGroup("Components"), SerializeField]
        private Button action1Button, action2Button;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        public static ModalPopup Instance { get; private set; }
        
        public bool Active => gameObject.activeSelf;
        
        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        #endregion

        #region Inspector ..............................................................................................

        #endregion

        #region Initialization .........................................................................................

        #endregion

        #region MonoBehaviours .........................................................................................

        protected override void OnAwake()
        {
            base.OnAwake();

            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                throw new Exception("ModalPopup is a singleton, there can only be one instance at a time.");
            }
            
            Hide();
        }

        #endregion

        #region General ................................................................................................

        public void Show(string heading, string message, string action1Label, UnityAction action1)
        {
            Show(heading, message, action1Label, action1, null, null);;
        }
        
        public void Show(string heading, string message, string action1Label, UnityAction action1, string action2Label, UnityAction action2)
        {
            headingText.text = heading;
            messageText.text = message;
            
            if(action1 == null) throw new Exception("Action1 cannot be null");
            
            action1Button.gameObject.SetActive(true);
            action1Button.GetComponentInChildren<TextMeshProUGUI>().text = action1Label;
            action1Button.onClick.AddListener(action1);
            
            gameObject.SetActive(true);

            if (action2 == null)
            {
                action2Button.gameObject.SetActive(false);
                
                return;
            }
            
            action2Button.gameObject.SetActive(true);
            action2Button.GetComponentInChildren<TextMeshProUGUI>().text = action2Label;
            action2Button.onClick.AddListener(action2);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            
            headingText.text = string.Empty;
            messageText.text = string.Empty;
            
            action1Button.onClick.RemoveAllListeners();
            action2Button.onClick.RemoveAllListeners();
            
            
        }
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

#endregion
    }
}