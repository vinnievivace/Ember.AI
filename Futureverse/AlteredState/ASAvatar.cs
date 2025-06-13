using EmberAI;
using EmberAI.Attributes;
using EmberAI.Core;
using EmberAI.Core.Util;
using EmberAI.Futureverse.AssetRegistry;
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

        // custom animator params, extend the core params defined in <see cref="EmberAI.Avatars.AvatarAnimator"/>
        private static readonly int HasBrainID = Animator.StringToHash("HasBrain");
        
        [BoxGroup("Components")]
        public ASForm Form;
        
        [BoxGroup("Components"), SerializeField]
        private ASMBrain Brain;
        
        [BoxGroup("Components"), SerializeField, ReadOnly]
        [Tooltip("Reference to the Forms Animator")]
        private Animator Animator;
        
        
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

            if (Form != null) Animator = Form.GetComponent<Animator>();
        }

        #endregion

        #region MonoBehaviours .........................................................................................

        protected override void OnUpdate()
        {
            base.OnUpdate();
            
            if(Form == null) return;

            Form.ControllerSystem.lookAtEnabled = Brain != null;
            
            if(Animator != null) Animator.SetBool(HasBrainID, Brain != null);
        }

        #endregion

        #region General ................................................................................................

        public void InitializeBrain(AssetItem data)
        {
            Brain = this.GetOrAddComponent<ASMBrain>();
        }
        
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

#endregion
    }
}