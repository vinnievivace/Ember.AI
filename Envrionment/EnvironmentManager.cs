using Core;
using EmberAI.Attributes;
using UnityEngine;

namespace EmberAI.Envrionment
{
    #if HDRP_ENABLED
    public class EnvironmentManager : EmberSingleton<EnvironmentManager>
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        [BoxGroup("Settings"), SerializeField]
        private EnvironmentSettings Settings;

        [BoxGroup("Lighting"), SerializeField] 
        private Light sun, avatarSpot;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        public LayerMask GroundLayer => Settings.GroundLayer;
        
        public Light Sun => sun;
        
        public float ShadowStrength => Settings.shadowStrength;
        
        public Light AvatarSpot => avatarSpot;
        
        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        #endregion

        #region Inspector ..............................................................................................

        public override string GetDocumentation()
        {
            string documentation = "- Ensure Active Quality Setting has a RenderPipeline Asset with Light Layers enabled." + "\n\n";
            
            documentation += "- Set Rendering LayerMask on AvatarLight, to default, plus a specific light layer" + "\n";
            documentation += "- if Rendering LayerMask is not visible, Open Preferences > Graphics" + "\n";
            documentation += "- set Advanced Properties to 'All Visible" + "\n";
            documentation += "- Set Rendering LayerMask to match, on all target Renderers" + "\n";
            
            return documentation;
        }

        #endregion

        #region Initialization .........................................................................................

        public override void InitializeDependencies()
        {
            base.InitializeDependencies();
            
            description = "Singleton for managing environmental settings, lighting etc";
        }
        
        #endregion

        #region MonoBehaviours .........................................................................................

        protected override void OnUpdate()
        {
            base.OnUpdate();
            
            if(Settings == null) return;

            if (sun != null)
            {
                sun.intensity = Settings.sunIntensity;
                sun.useColorTemperature = true;
                sun.colorTemperature = Settings.sunTemperature;
            }

            if (avatarSpot != null)
            {
                avatarSpot.intensity = Settings.avatarSpotIntensity;
                avatarSpot.type = LightType.Box;
                avatarSpot.areaSize = new Vector2(Settings.avatarSpotWidth, Settings.avatarSpotHeight);
                
                avatarSpot.transform.localPosition = new Vector3(0, Settings.avatarSpotYOffset, Settings.avatarSpotDistance);
                avatarSpot.renderingLayerMask = Settings.AvatarLightLayer;
                avatarSpot.useColorTemperature = true;
                avatarSpot.colorTemperature = Settings.avatarSpotTemperature;
            }
        }

        #endregion

        #region General ................................................................................................

        /// <summary>
        /// Applies spot lighting settings to all child renderers of the target GameObject.
        /// </summary>
        /// <param name="target">
        /// The GameObject to which the spot lighting settings should be applied. All Renderer components
        /// in the children of this GameObject will have their rendering layer mask updated.
        /// </param>
        public void ApplySpotLighting(Component target)
        {
            foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>())
            {
                renderer.renderingLayerMask = Settings.AvatarLightLayer;
            }
        }

        #endregion

        #region Event Handlers .........................................................................................

        #endregion

        #endregion
    }
    #endif
}