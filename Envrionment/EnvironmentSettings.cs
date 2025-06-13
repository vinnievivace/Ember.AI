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
        
        [BoxGroup("Avatar Light")]
        public LightType avatarLightType = LightType.Box;
        
        [BoxGroup("Avatar Light"), Tooltip("Only applicable when using HDRP")] 
        public RenderingLayerMask AvatarLightLayer;
        
        [BoxGroup("Avatar Light")]
        public int avatarLightIntensity = 6500, avatarLightTemperature = 5000;

        [BoxGroup("Avatar Light")]
        public Vector3 avatarLightOffset = new Vector3(0, 1.5f, 0);
        
        [BoxGroup("Avatar Light")]
        public float avatarLightWidth = 10f, avatarLightHeight = 10, avatarLightDistance = 0; 

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