using Core;
using EmberAI.Attributes;
using EmberAI.Avatars;
using UnityEngine;

namespace EmberAI.Envrionment
{
    #if HDRP_ENABLED
    public class HDEnvironmentManager : EmberSingleton<HDEnvironmentManager>
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        [BoxGroup("Settings"), SerializeField]
        private HDEnvironmentSettings Settings;

        [BoxGroup("Lighting"), SerializeField] 
        private RenderingLayerMask avatarLightLayer;

        [BoxGroup("Lighting"), SerializeField] 
        private Light sun, avatarSpot;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        #endregion

        #region Inspector ..............................................................................................

        public override void InitializeDependencies()
        {
            base.InitializeDependencies();
            
            description = "Singleton for managing HDRP specific environmental settings, lighting etc";
        }

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
                avatarSpot.renderingLayerMask = avatarLightLayer;
            }
        }

        #endregion

        #region General ................................................................................................

        // TODO refine this, very rough! should check for HDRP, should be more generic etc
        public void SetAvatar(AvatarAnimator avatar)
        {
            foreach (Renderer renderer in avatar.GetComponentsInChildren<Renderer>())
            {
                renderer.renderingLayerMask = avatarLightLayer;
            }
        }
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

        #endregion
    }
    #endif
}