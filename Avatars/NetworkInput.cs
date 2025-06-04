namespace EmberAI.Avatars
{
    using UnityEngine;

    public class NetworkInput : BaseCharacterInput
    {
        // Suppose you receive these from your networking layer
        public Vector2 syncedMove;
        public bool    syncedJump;
        public bool    syncedRun;
        public bool    syncedCrouch;

        public override Vector2 ReadMovementInput()
        {
            return syncedMove;
        }

        public override bool JumpTriggered()
        {
            return syncedJump;
        }

        public override bool IsRunning()
        {
            return syncedRun;
        }

        public override bool IsCrouching()
        {
            return syncedCrouch;
        }
    }

}