using EmberAI.Attributes;
using UnityEngine;

namespace EmberAI.Avatars
{
    [CreateAssetMenu(menuName = "Characters/Character Settings")]
    public class CharacterSettings : BaseData
    {
        [BoxGroup("Movement")]
        public float walkSpeed    = 3f;
        public float runSpeed     = 6f;
        public float crouchSpeed  = 1.5f;

        [BoxGroup("Jumping")]
        public bool  canJump      = true;
        public float jumpForce    = 7f;
        public float gravity      = -9.81f;

        [BoxGroup("Crouch")]
        public bool  canCrouch    = true;
        public float crouchHeight = 1f;
    }

}