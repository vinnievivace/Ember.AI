using Core;
using EmberAI.Attributes;
using EmberAI.Core;
using UnityEngine;

namespace EmberAI.UI
{
    [CreateAssetMenu(fileName = "UISettings", menuName = EmberAISystem.MenuPath + "/Settings/UISettings")]
    public class UISettings : BaseData
    {
        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        [BoxGroup("Audio")]
        public AudioClip defaultShowSound, defaultHideSound, defaultHoverSound, defaultClickSound;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        #endregion

        #region Inspector ..............................................................................................

        #endregion

        #region Initialization .........................................................................................

        #endregion

        #region MonoBehaviours .........................................................................................

        #endregion

        #region General ................................................................................................

        #endregion

        #region Event Handlers .........................................................................................

        #endregion

#endregion
    }
}