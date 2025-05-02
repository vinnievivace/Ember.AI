using System;
using EmberAI.Attributes;
using UnityEngine;

namespace EmberAI
{
    public abstract class EmberBehaviour : MonoBehaviour
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

        /// <summary>
        /// Initialize any dependencies that are instantiated when this component is created. Should only be called in Edit mode.
        /// </summary>
        public virtual void InitializeDependencies()
        {
            if(Application.isPlaying) return;
            
            if(name is "GameObject" or "") name = "Ember";
            
        }

        /// <summary>
        /// Clean up any dependencies that are instantiated when this component is created. Call prior to Destroying this component.
        /// </summary>
        public virtual void DestroyDependencies()
        {
            // override       
        }
        
        #endregion

        #region MonoBehaviours .........................................................................................

        // sealing the core MonoBehaviour methods here to allow customization
        
        [SealedMethod]
        private void Awake() { OnAwake(); }

        [SealedMethod]
        private void Start() { OnStart(); }

        [SealedMethod] 
        private void Update() { OnUpdate(); }

        [SealedMethod]
        private void Reset() { OnReset(); }

        [SealedMethod]
        private void OnDestroy()
        {
            DestroyDependencies();
            OnDestroyed();
        }

        protected virtual void OnAwake()
        {
            // override
        }

        protected virtual void OnStart()
        {
            // override
        }

        protected virtual void OnUpdate()
        {
            // override       
        }

        protected virtual void OnDestroyed()
        {
            
        }

        protected virtual void OnReset()
        {
            InitializeDependencies();
        }
        
        
        #endregion

        #region General ................................................................................................

        #endregion

        #region Event Handlers .........................................................................................

        #endregion

        #endregion
    }
}