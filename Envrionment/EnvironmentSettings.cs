using Core;
using EmberAI.Attributes;
using EmberAI.Attributes.EmberAI.Attributes;
using EmberAI.Core;
using UnityEngine;

namespace EmberAI.Envrionment
{
    [CreateAssetMenu(fileName = "EnvironmentSettings", menuName = EmberAISystem.MenuPath + "/Settings/EnvironmentSettings", order = 1)]
    public class EnvironmentSettings : BaseData
    {
       #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        [BoxGroup("Ground")]
        public LayerMask GroundLayer;

        [BoxGroup("Ground"), Tooltip("Limit Avatar ability to scale Ground/Terrain slopes")]
        public int slopeLimit = 30;
        
        [BoxGroup("Sun")] 
        public int sunIntensity = 80500, sunTemperature = 5050;
        
        [BoxGroup("Sun")] 
        [Range(0,1)]
        public float shadowStrength = 0.5f;
        
        [BoxGroup("AvatarSpot")] 
        public RenderingLayerMask AvatarLightLayer;
        
        [BoxGroup("AvatarSpot")]
        public int avatarSpotIntensity = 6500, avatarSpotTemperature = 5000;

        [BoxGroup("AvatarSpot")] 
        public float avatarSpotWidth = 1.5f, avatarSpotHeight = 5, avatarSpotDistance = 5, avatarSpotYOffset = 1.5f;

        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        #endregion

        #region Inspector ..............................................................................................

        [ButtonGroup("Tools", "Apply", "Applies the updated settings to the environment")]
        private void Apply()
        {
            EnvironmentManager.Instance.ApplySettings(this);
        }
        
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