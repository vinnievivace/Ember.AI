using System;
using EmberAI;
using EmberAI.Core;
using UnityEngine;

namespace Core
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class EmberSingleton<T> : EmberBehaviour where T : EmberBehaviour
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        private static T _instance;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        public static T Instance => GetInstance();
        
        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        private static T GetInstance()
        {
            if (_instance != null) return _instance;

            _instance = FindFirstObjectByType<T>();

            if (_instance == null)
            {
                T resourcePrefab = Resources.Load<T>($"Singletons/{typeof(T).Name}");

                if (resourcePrefab != null)
                {
                    _instance = Instantiate(resourcePrefab);
                    _instance.gameObject.name = typeof(T).Name;
                    DontDestroyOnLoad(_instance.gameObject);    
                }
            }

            if (_instance != null) (_instance as EmberSingleton<T>)?.OnInstanceActivated();

            return _instance;
        }
        
        #endregion

        #region Inspector ..............................................................................................

        #endregion

        #region Initialization .........................................................................................

        protected virtual void OnInstanceActivated() 
        {
            // for override
        }

        public static void InstantiateIfMissing(Component parent)
        {
            if (GetInstance() == null) parent.GetOrAddComponent<T>();
        }
        
        #endregion

        #region MonoBehaviours .........................................................................................

        protected override void OnAwake()
        {
            base.OnAwake();
            
            // ensure there is only a single instance
            if (FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length > 1)
            {
                throw new Exception(typeof(T).Name + " is a singleton, but more than one instance exists in active scene");
            }
            
            // GetInstance onAwake to ensure OnInitialize is called immediately, rather than waiting for the first time the Instance is referenced
            GetInstance();
        }
        
        #endregion

        #region General ................................................................................................

        #endregion

        #region Event Handlers .........................................................................................

        #endregion

#endregion
    }
}