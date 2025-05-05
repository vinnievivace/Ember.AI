using System;
using System.Collections.Generic;
using System.Diagnostics;
using Core;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace EmberAI.Core
{
    public static class CallbackManager
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////
        
        #endregion
        
        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////
        
        #endregion
        
        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        private static Stopwatch _stopWatch;
        
        #endregion
        
        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////

        public static double ElapsedMilliSeconds => _stopWatch.ElapsedMilliseconds;

        internal static List<CallbackDefinition> Callbacks { get; set; } = new List<CallbackDefinition>();
        
        #endregion
        
        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////
        
        #region Public .................................................................................................

        /// <summary>
        /// Initializes a repeating Callback
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="intervalInSeconds"></param>
        /// <param name="onIntervalCallback"></param>
        [UsedImplicitly]
        public static void AddRepeating(object owner, float intervalInSeconds, Action onIntervalCallback)
        {
            Add(owner, intervalInSeconds, true, onIntervalCallback);
        }
        
        /// <summary>
        /// Initializes a one off Callback
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="delayInSeconds"></param>
        /// <param name="onIntervalCallback"></param>
        [UsedImplicitly]
        public static void AddOneOff(object owner, float delayInSeconds, Action onIntervalCallback)
        {
            // can be convenient to still use Callback manager with zero delay, so handle gracefully
            // (and in a way that will still work in Edit mode)
            if (delayInSeconds == 0)
            {
                onIntervalCallback.Invoke();
                
                return;
            }
            
            Add(owner, delayInSeconds, false, onIntervalCallback);

        }

        private static void Add(object owner, float intervalInSeconds, bool repeating, Action onIntervalCallback)
        {
            if(onIntervalCallback == null) throw new Exception("onIntervalCallback cannot be null");
            
            // can be convenient to still use Callback manager with zero delay, so handle gracefully
            // (and in a way that will still work in Edit mode)
            if (intervalInSeconds == 0)
            {
                onIntervalCallback.Invoke();
                
                return;
            }
            
            // ensure an instance of the CallbackRunner exists to call the Update method
            if (CallbackRunner.Instance == null)
            {
                CallbackRunner runner = new GameObject().AddComponent<CallbackRunner>();
                
                _stopWatch = new Stopwatch();
                _stopWatch.Start();

                runner.gameObject.hideFlags = HideFlags.HideAndDontSave;
            }
            
            // ignore if already added
            if (Callbacks.Find(i => i.owner == owner && i.callback == onIntervalCallback) != null)
            {
                return;
            }
            
            Callbacks.Add(new CallbackDefinition(owner, onIntervalCallback, intervalInSeconds * 1000, repeating));
        }

        /// <summary>
        /// Remove the specific <see cref="Action"/> callback
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="callback"></param>
        public static void Remove(object owner, Action callback)
        {
            var callbacks = Callbacks.GetCopy();
            
            callbacks.Remove(callbacks.Find(i=> i.owner == owner && i.callback == callback));

            Callbacks = callbacks;
        }

        /// <summary>
        /// Remove all defined <see cref="Action"/> callbacks for supplied Owner
        /// </summary>
        /// <param name="owner"></param>
        public static void RemoveAll(object owner)
        {
            var callbacks = Callbacks.GetCopy();

            callbacks.RemoveAll(i => i.owner == owner);
            
            Callbacks = callbacks;
        }
        
        /// <summary>
        /// Called by <see cref="CallbackRunner"/> in Update loop, runs all Callback Definitions
        /// </summary>
        private static void DoUpdate()
        {
            var itemsToRemove = new List<CallbackDefinition>();
            var itemsToUpdate = new List<CallbackDefinition>();
            
            foreach (CallbackDefinition item in Callbacks.GetCopy())
            {
                try
                {
                    if (item.nextInvokeTimeInMilliSeconds <= ElapsedMilliSeconds)
                    {
                        item.callback();

                        if (item.repeating)
                        {
                            itemsToUpdate.Add(item);
                        }
                        else
                        {
                            itemsToRemove.Add(item);
                        }
                    }
                }
                catch (Exception e)
                {
                    // This can often be a point where exceptions further back in the stack are found
                    Debug.LogWarning("Exception when trying to run callback on " + item.owner + ", removing all callbacks");
                    Debug.LogError(e.StackTrace);
                    
                    RemoveAll(item.owner);
                }
            }

            foreach (CallbackDefinition item in itemsToRemove)
            {
                Callbacks.Remove(item);
            }

            foreach (CallbackDefinition item in itemsToUpdate)
            {
                CallbackDefinition updateItem = Callbacks.Find(i => i.ID == item.ID);

                updateItem.nextInvokeTimeInMilliSeconds = ElapsedMilliSeconds + item.intervalInMilliSeconds;
            }
        }
        
        #endregion
        
        #region Private ................................................................................................
        
        #endregion
        
        #endregion


        internal class CallbackDefinition
        {
            public readonly string ID;
            
            public readonly double intervalInMilliSeconds;
            
            public readonly object owner;
            public readonly Action callback;
            public readonly bool repeating;
            public double nextInvokeTimeInMilliSeconds;

            public CallbackDefinition(object owner, Action callback, double intervalInMilliSeconds, bool repeating = false)
            {
                ID = Guid.NewGuid().ToString();
                
                this.owner = owner;
                this.callback = callback;
                this.repeating = repeating;
                this.intervalInMilliSeconds = intervalInMilliSeconds;
                
                nextInvokeTimeInMilliSeconds = ElapsedMilliSeconds + intervalInMilliSeconds;
            }
        }

        /// <summary>
        /// Automatically instanced MonoBehaviour, sole purpose to call the <see cref="CallbackManager"/> update method
        /// </summary>
        public class CallbackRunner : EmberSingleton<CallbackRunner>
        {
            [FormerlySerializedAs("activeCallbacks")] public float oneOffCallbacks;
            public float repeatingCallbacks;


            protected override void OnStart()
            {
                base.OnStart();
            
                DontDestroyOnLoad(this);

                name = "CallBackRunner";

            }

            protected override void OnUpdate()
            {
                base.OnUpdate();
            
                DoUpdate();

                repeatingCallbacks = Callbacks.FindAll(i => i.repeating).Count;
                oneOffCallbacks = Callbacks.Count - repeatingCallbacks;
            }
        }
    }
}