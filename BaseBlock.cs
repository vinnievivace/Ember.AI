using EmberAI.Attributes;
using UnityEngine;

namespace EmberAI
{
    public abstract class BaseBlock : MonoBehaviour
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        [BoxGroup(""), InspectorText] 
        public string description = "Building the Open Metaverse with Blocks.";
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        #endregion

        #region Inspector ..............................................................................................

        #endregion

        #region Initialization .........................................................................................

        public virtual void EditorRebuild()
        {
            if(Application.isPlaying) return;
            
            if(name is "GameObject" or "") name = "BaseBlock";
            
        }
        
        #endregion

        #region MonoBehaviours .........................................................................................

        // sealing the core MonoBehaviour methods here to allow customization
        
        [SealedMethod]
        private void Awake() { OnAwake(); }

        [SealedMethod]
        private void Start() { OnStart(); }

        [SealedMethod] 
        private void OnDestroy() { OnDestroyed(); }

        protected virtual void OnAwake()
        {
            // override
        }

        protected virtual void OnStart()
        {
            // override
        }

        protected virtual void OnDestroyed()
        {
            // override
        }
        
        
        #endregion

        #region General ................................................................................................

        #endregion

        #region Event Handlers .........................................................................................

        #endregion

        #endregion
    }
}