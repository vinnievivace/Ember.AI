using System;

namespace EmberAI.Avatars
{
    [Serializable]
    public class AvatarAnimatorState
    {
        public float currentSpeed;
        public float maxSpeed;
        public bool jump;
        public bool crouch;
        public bool dance;
        public bool isGrounded;
        public float verticalVelocity;
    }
}