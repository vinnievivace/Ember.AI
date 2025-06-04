using UnityEngine;

namespace EmberAI.Avatars
{
    public abstract class BaseCharacterInput : EmberBehaviour
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

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

        #endregion

        #region General ................................................................................................

        public virtual Vector2 ReadMovementInput()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool JumpTriggered()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsDancing()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsRunning()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsCrouching()
        {
            throw new System.NotImplementedException();
        }
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

#endregion


    }
}