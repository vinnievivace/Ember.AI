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

        protected enum LogLevel { Log, Warning, Error };
        
        
        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        [BoxGroup(""), InspectorText] 
        public string description = "Tools for Metaverse / Blockchain / Extended Reality / AI experiences.";
        
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
        private void FixedUpdate() { OnFixedUpdate(); }

        [SealedMethod]
        private void LateUpdate() { OnLateUpdate(); }

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

        protected virtual void OnFixedUpdate()
        {
            // override      
        }
        
        protected virtual void OnLateUpdate()
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

        protected void Log(LogLevel level, string message)
        {
            // this will be for the per instance logging feature, but for now, just use console
            switch (level)
            {
                case LogLevel.Log: Debug.Log(message); break;
                case LogLevel.Warning: Debug.LogWarning(message); break;
                case LogLevel.Error: Debug.LogError(message); break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
        
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

        #endregion
    }
}