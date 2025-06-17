using Core;
using EmberAI.Attributes;
using EmberAI.Cameras;
using EmberAI.Core.Util;
using UnityEngine;

namespace EmberAI.Envrionment
{
    
    public class EnvironmentManager : EmberSingleton<EnvironmentManager>
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        [BoxGroup("Settings"), SerializeField, OnValueChanged(nameof(DoApplySettings))]
        private EnvironmentSettings settings;

        [BoxGroup("Lighting"), SerializeField, OnValueChanged(nameof(DoApplySettings))] 
        private Light sun, avatarSpot;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        public static EnvironmentSettings Settings => Instance.settings;
        
        public Light Sun => sun;
        
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

        private void DoApplySettings()
        {
            ApplySettings(settings);
        }

        #endregion

        #region Initialization .........................................................................................

        public override void EditModeInitialize()
        {
            base.EditModeInitialize();
            
            description = "Singleton for managing environmental settings, lighting etc";
        }
        
        #endregion

        #region MonoBehaviours .........................................................................................

        protected override void OnAwake()
        {
            base.OnAwake();
            
            if(settings == null) throw new System.Exception("EnvironmentSettings is missing");
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            
            ApplySettings(settings);
        }

        #endregion

        #region General ................................................................................................

        public void ApplySettings(EnvironmentSettings settings)
        {
            this.settings = settings;
            
            if(this.settings == null) return;

            if (sun != null)
            {
                sun.intensity = this.settings.sunIntensity;
                sun.useColorTemperature = true;
                sun.colorTemperature = this.settings.sunTemperature;
            }

            if (avatarSpot != null)
            {
                avatarSpot.intensity = this.settings.avatarLightIntensity;
                avatarSpot.type = this.settings.avatarLightType;
                avatarSpot.areaSize = new Vector2(this.settings.avatarLightWidth, this.settings.avatarLightHeight);
                
                avatarSpot.renderingLayerMask = this.settings.AvatarLightLayer;
                avatarSpot.useColorTemperature = true;
                avatarSpot.colorTemperature = this.settings.avatarLightTemperature;
                
                if(ThirdPersonCamera.Instance == null || ThirdPersonCamera.Instance.Target == null) return;
                
                // rotate the avatar spot to always face the avatar (using the Camera Target Rotation) and offset as per settings.
                Quaternion targetRotation = ThirdPersonCamera.Instance.TargetRotation;
                
                TransformUtil.PositionRelativeTo(ThirdPersonCamera.Instance.Target, AvatarSpot, targetRotation, settings.avatarLightOffset, settings.avatarLightDistance);
            }
        }
        
        /// <summary>
        /// Applies spot lighting settings to all child renderers of the target GameObject.
        /// </summary>
        /// <param name="target">
        /// The GameObject to which the spot lighting settings should be applied. All Renderer components
        /// in the children of this GameObject will have their rendering layer mask updated.
        /// </param>
        public void ApplyAvatarLighting(Component target)
        {
            foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>())
            {
                renderer.renderingLayerMask = settings.AvatarLightLayer;
            }
        }

        /// <summary>
        /// Retrieves the index of the ground layer based on the associated layer mask in the environment settings.
        /// </summary>
        /// <returns>
        /// The index of the ground layer as an integer, calculated from the layer mask defined in the environment settings.
        /// </returns>
        public int GetGroundLayerIndex()
        {
            return Mathf.RoundToInt(Mathf.Log(settings.GroundLayer.value, 2));
        }

        #endregion

        #region Event Handlers .........................................................................................

        #endregion

        #endregion
    }
}