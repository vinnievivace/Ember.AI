using UnityEngine;

namespace EmberAI.Avatars
{
    public interface ICharacterInput
    {
        /// <summary>
        /// Called every frame to get the desired move vector (x,z).
        /// </summary>
        Vector2  ReadMovementInput();

        /// <summary>
        /// True only on the frame jump was requested.
        /// </summary>
        bool     JumpRequested();

        /// <summary>
        /// True if character should toggle run.
        /// </summary>
        bool     IsRunning();

        /// <summary>
        /// True if character should toggle crouch.
        /// </summary>
        bool     IsCrouching();
    }

}