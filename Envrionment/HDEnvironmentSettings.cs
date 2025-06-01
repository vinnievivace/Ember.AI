using Core;
using EmberAI.Attributes;
using EmberAI.Core;
using UnityEngine;

namespace EmberAI.Envrionment
{
    [CreateAssetMenu(fileName = "HDEnvironmentSettings", menuName = EmberAISystem.MenuPath + "/Settings/HDEnvironmentSettings", order = 1)]
    public class HDEnvironmentSettings : BaseData
    {
       #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        [BoxGroup("Sun")] 
        public int sunIntensity = 80500, sunTemperature = 5050;
        
        [BoxGroup("AvatarSpot")]
        public int avatarSpotIntensity = 6500;

        [BoxGroup("AvatarSpot")] 
        public float avatarSpotWidth = 1.5f, avatarSpotHeight = 5, avatarSpotDistance = 5, avatarSpotYOffset = 1.5f;

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