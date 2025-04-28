namespace EmberAI.Avatars
{
    using UnityEngine;

    public class NetworkInput : MonoBehaviour, ICharacterInput
    {
        // Suppose you receive these from your networking layer
        public Vector2 syncedMove;
        public bool    syncedJump;
        public bool    syncedRun;
        public bool    syncedCrouch;

        Vector2 ICharacterInput.ReadMovementInput() => syncedMove;
        bool    ICharacterInput.JumpRequested()     => syncedJump;
        bool    ICharacterInput.IsRunning()         => syncedRun;
        bool    ICharacterInput.IsCrouching()       => syncedCrouch;
    }

}