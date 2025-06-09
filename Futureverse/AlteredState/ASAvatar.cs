using EmberAI;
using EmberAI.Attributes;
using EmberAI.Core.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Futureverse.AlteredState
{
    /// <summary>
    /// <see cref="ASAvatar"/> combines an <see cref="ASForm"/> with an <see cref="ASMBrain"/> to create an intelligent agent.
    /// </summary>
    public class ASAvatar : EmberBehaviour
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        [BoxGroup("Components")]
        public ASForm Form;
        
        [BoxGroup("Components"), SerializeField]
        public ASMBrain Brain;
        
        
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

        protected override void OnUpdate()
        {
            base.OnUpdate();
            
            if(Form == null) return;

            Form.ControllerSystem.lookAtEnabled = Brain != null;
        }

        #endregion

        #region General ................................................................................................

        #endregion

        #region Event Handlers .........................................................................................

        #endregion

#endregion
    }
}