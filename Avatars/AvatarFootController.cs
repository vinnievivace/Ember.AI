using EmberAI.Attributes;
using EmberAI.Core;
using EmberAI.Envrionment;
using UnityEngine;

namespace EmberAI.Avatars
{
    public class AvatarFootController : EmberBehaviour
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        [BoxGroup("Components"), ReadOnly, SerializeField]
        private CharacterControllerSystem characterController;
        
        [BoxGroup("Components"), ReadOnly, SerializeField]
        private Animator animator;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        public bool IsGrounded { get; private set; }
        public Vector3 GroundNormal { get; private set; } = Vector3.up;
        
        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        #endregion

        #region Inspector ..............................................................................................

        #endregion

        #region Initialization .........................................................................................

        public override void EditModeInitialize()
        {
            base.EditModeInitialize();

            characterController = GetComponent<CharacterControllerSystem>();
            animator = this.GetOrAddComponent<Animator>();
        }

        #endregion

        #region MonoBehaviours .........................................................................................

        protected override void OnAwake()
        {
            base.OnAwake();
            
            if(characterController == null) throw new System.Exception("CharacterControllerSystem is missing");
            if(characterController.settings == null) throw new System.Exception("CharacterControllerSystem.settings is missing");
        }

        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
            
            UpdateGroundCheck();
        }
        
        private void OnAnimatorIK(int layerIndex)
        {
            ApplyFootIK(AvatarIKGoal.LeftFoot, AvatarIKHint.LeftKnee);
            ApplyFootIK(AvatarIKGoal.RightFoot, AvatarIKHint.RightKnee);
        }
        
        #endregion

        #region General ................................................................................................

        private void ApplyFootIK(AvatarIKGoal foot, AvatarIKHint kneeHint)
        {
            animator.SetIKPositionWeight(foot, characterController.settings.ikWeight);
            animator.SetIKRotationWeight(foot, characterController.settings.ikWeight);
            animator.SetIKHintPositionWeight(kneeHint, characterController.settings.kneeHintWeight);

            Vector3 footPos = animator.GetIKPosition(foot);
            Quaternion footRot = animator.GetIKRotation(foot);

            if (Physics.Raycast(footPos + Vector3.up * characterController.settings.raycastDistance, Vector3.down,
                    out RaycastHit hit, characterController.settings.raycastDistance * 2f, EnvironmentManager.Settings.GroundLayer))
            {
                Vector3 targetPos = hit.point + Vector3.up * characterController.settings.footHeightOffset;
                Quaternion targetRot =
                    Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);

                animator.SetIKPosition(foot, Vector3.Lerp(footPos, targetPos, characterController.settings.ikWeight));
                animator.SetIKRotation(foot, Quaternion.Slerp(footRot, targetRot, characterController.settings.ikWeight));

                Transform thigh = animator.GetBoneTransform(
                    foot == AvatarIKGoal.LeftFoot ? HumanBodyBones.LeftUpperLeg : HumanBodyBones.RightUpperLeg);

                Vector3 hintDirection = transform.forward * characterController.settings.kneeHintForward +
                                        transform.right *
                                        (foot == AvatarIKGoal.LeftFoot ? -characterController.settings.kneeHintOutward : characterController.settings.kneeHintOutward);

                Vector3 hintPos = thigh.position + hintDirection;
                animator.SetIKHintPosition(kneeHint, hintPos);
            }
            else
            {
                animator.SetIKPosition(foot, footPos);
                animator.SetIKRotation(foot, footRot);
            }
        }
        

        public void PlaceOnGround()
        {
            Ray ray = new Ray(transform.position + Vector3.up * 1f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 5f, EnvironmentManager.Settings.GroundLayer))
            {
                transform.position = hit.point;
            }
        }
        
        private void UpdateGroundCheck()
        {
            float groundCheckDistance = characterController.settings.raycastDistance;
            Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, groundCheckDistance, EnvironmentManager.Settings.GroundLayer))
            {
                IsGrounded = true;
                GroundNormal = hit.normal;
            }
            else
            {
                IsGrounded = false;
                GroundNormal = Vector3.up;
            }
        }

        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

#endregion
    }
}