using EmberAI.Attributes;
using EmberAI.Core;
using EmberAI.Envrionment;
using UnityEngine;

namespace EmberAI.Avatars
{
    public class AvatarFootController : EmberBehaviour
    {
        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Last smoothed pelvis offset.</summary>
        private float _lastPelvisOffset = 0f;

        [BoxGroup("Components"), ReadOnly, SerializeField]
        private CharacterControllerSystem characterController;

        [BoxGroup("Components"), ReadOnly, SerializeField]
        private Animator animator;
        
        [BoxGroup("Components"), ReadOnly, SerializeField]
        private AvatarFoot leftFoot;

        [BoxGroup("Components"), ReadOnly, SerializeField]
        private AvatarFoot rightFoot;

        [BoxGroup("State"), ReadOnly, SerializeField]
        public bool isGrounded;

        private float _lastGroundedTime = float.NegativeInfinity;

        [BoxGroup("Debug"), SerializeField]
        private Vector3 placeAtPosition;

        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        /// <summary>Public accessor for current grounded state, including grace period.</summary>
        public bool IsGrounded => isGrounded;

        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Initialization .........................................................................................

        public override void EditModeInitialize()
        {
            base.EditModeInitialize();
            characterController = GetComponent<CharacterControllerSystem>();
            animator = this.GetOrAddComponent<Animator>();
        }

        public void Initialize()
        {
            leftFoot = AvatarFoot.Create(characterController, HumanBodyBones.LeftFoot);
            rightFoot = AvatarFoot.Create(characterController, HumanBodyBones.RightFoot);
        }
        
        #endregion
        
        #region MonoBehaviours ........................................................................................

        protected override void OnAwake()
        {
            base.OnAwake();
            if (characterController == null) throw new System.Exception("CharacterControllerSystem is missing");
            if (characterController.settings == null) throw new System.Exception("CharacterControllerSystem.settings is missing");
        }
        
        protected override void OnUpdate()
        {
            base.OnUpdate();

            // If feet not yet set up, assume grounded
            if (leftFoot == null || rightFoot == null)
            {
                isGrounded = true;
                return;
            }

            // Update last grounded timestamp when either foot touches ground
            if (leftFoot.IsGrounded || rightFoot.IsGrounded)
            {
                _lastGroundedTime = Time.time;
            }

            isGrounded = (Time.time - _lastGroundedTime) <= characterController.settings.IsGroundedTolerance;
        }

        #endregion

        #region IK and Positioning ................................................................................

        private void OnAnimatorIK(int layerIndex)
        {
            if (!characterController.settings.footIKEnabled) return;
            
            AdjustPelvisHeight();
        }

        private void AdjustPelvisHeight()
        {
            if (!characterController.settings.enablePelvisAdjustment) return;

            Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            
            if (hips == null) return;

            // Use the smaller blend (lowest foot) to calculate pelvis drop
            float lowestY = Mathf.Min(leftFoot.Blend, rightFoot.Blend);
            float baseY = transform.position.y + 0.9f;
            float targetOffset = Mathf.Clamp(lowestY - baseY, -characterController.settings.pelvisAdjustmentAmount, 0f);

            _lastPelvisOffset = Mathf.Lerp(_lastPelvisOffset, targetOffset, Time.deltaTime * characterController.settings.pelvisAdjustmentSpeed);

            Vector3 lp = hips.localPosition;
            
            hips.localPosition = new Vector3(lp.x, lp.y + _lastPelvisOffset, lp.z);
        }

        #endregion

        #region UTILITIES ...............................................................................................

        public void PlaceOnGround(Vector3 position)
        {
            if (Physics.Raycast(position + Vector3.up, Vector3.down, out RaycastHit hit, 5f,
                    EnvironmentManager.Settings.GroundLayer))
            {
                transform.position = hit.point;
            }
        }

        #endregion

        #endregion  // end METHODS
    }
}
