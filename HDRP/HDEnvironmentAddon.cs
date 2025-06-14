#if HDRP_ENABLED
using Core;
using EmberAI.Envrionment;
using UnityEngine.Rendering.HighDefinition;


namespace EmberAI.HDRP
{
    
    public class HDEnvironmentAddon : EmberSingleton<HDEnvironmentAddon>
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        private HDAdditionalLightData _sunLightData;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

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
            
            description = "Singleton  additional HDRP specific environmental handling";
        }
        
        #endregion

        #region MonoBehaviours .........................................................................................

        protected override void OnAwake()
        {
            base.OnAwake();
            
            if(EnvironmentManager.Instance == null) throw new System.Exception("EnvironmentManager is missing");
            
            if(EnvironmentManager.Instance.Sun == null) throw new System.Exception("EnvironmentManager.Sun is missing");
            
            _sunLightData = EnvironmentManager.Instance.Sun.GetComponent<HDAdditionalLightData>();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            _sunLightData.shadowDimmer = EnvironmentManager.Settings.shadowStrength;
        }

        #endregion

        #region General ................................................................................................

        #endregion

        #region Event Handlers .........................................................................................

        #endregion

        #endregion
    }
}
#endif