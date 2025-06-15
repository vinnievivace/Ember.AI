using EmberAI.Attributes;
using EmberAI.Core;
using EmberAI.Envrionment;
using UnityEngine;

namespace EmberAI.Avatars
{
    public class AvatarFoot : EmberBehaviour
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        [BoxGroup("Settings"), Tooltip("Layer mask for ground detection")]
        public LayerMask groundLayers;

        [BoxGroup("Components"), ReadOnly, SerializeField]
        private SphereCollider collider;

        [BoxGroup("Components"), ReadOnly, SerializeField]
        private Animator animator;

        [BoxGroup("Components"), ReadOnly, SerializeField]
        private Transform footBone;

        [BoxGroup("State"), ReadOnly, SerializeField]
        private int groundCollisionCount;


        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        private float ActiveBlend { get; set; }
        private CharacterSettings Settings { get; set; }
        public bool IsGrounded => groundCollisionCount > 0;

        public float Blend { get; private set; }


        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        public static AvatarFoot Create(CharacterControllerSystem characterController, HumanBodyBones bone)
        {
            Animator animator = characterController.GetComponent<Animator>();

            if (animator == null)
                throw new System.Exception("Could not find animator on " + characterController.gameObject);

            Transform footBone = animator.GetBoneTransform(bone);

            if (footBone == null) throw new System.Exception("Could not find bone transform for " + bone);

            AvatarFoot foot = footBone.GetOrAddComponent<AvatarFoot>();
            SphereCollider collider = foot.GetOrAddComponent<SphereCollider>();
            Rigidbody rigidBody = foot.GetOrAddComponent<Rigidbody>();

            foot.animator = animator;
            foot.Settings = characterController.settings;
            foot.transform.localPosition = new Vector3(0f, foot.Settings.footContactColliderYOffset, 0f);
            foot.groundLayers = EnvironmentManager.Settings.GroundLayer;

            collider.isTrigger = true;
            collider.radius = characterController.settings.footContactColliderRadius;

            rigidBody.isKinematic = true;
            rigidBody.useGravity = false;

            return foot;
        }

        #endregion

        #region Inspector ..............................................................................................

        #endregion

        #region Initialization .........................................................................................

        #endregion

        #region MonoBehaviours .........................................................................................

        protected override void OnAwake()
        {
            base.OnAwake();

            collider = this.GetOrAddComponent<SphereCollider>();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (Settings == null) return;

            collider.radius = Settings.footContactColliderRadius;
            
            transform.localPosition = new Vector3(0f, Settings.footContactColliderYOffset, 0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((groundLayers & (1 << other.gameObject.layer)) != 0)
            {
                Debug.Log($"{gameObject.name} touched {other.name}");

                groundCollisionCount++;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if ((groundLayers & (1 << other.gameObject.layer)) != 0) groundCollisionCount--;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, collider.radius);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!Settings.footIKEnabled) return;

            ApplyFootIK(AvatarIKGoal.LeftFoot, AvatarIKHint.LeftKnee);
            ApplyFootIK(AvatarIKGoal.RightFoot, AvatarIKHint.RightKnee);
        }

        #endregion

        #region General ................................................................................................

        private void ApplyFootIK(AvatarIKGoal foot, AvatarIKHint kneeHint)
        {
            float targetBlend = (IsGrounded) ? 1f : 0f;
            float blendSpeed = Settings.pelvisAdjustmentSpeed;

            ActiveBlend = Mathf.MoveTowards(ActiveBlend, targetBlend, Time.deltaTime * blendSpeed);

            float ikWeight = Settings.ikWeight * ActiveBlend;

            animator.SetIKPositionWeight(foot, ikWeight);
            animator.SetIKRotationWeight(foot, ikWeight);
            animator.SetIKHintPositionWeight(kneeHint, Settings.kneeHintWeight * ActiveBlend);

            if (ActiveBlend <= 0f) return;

            Vector3 footPos = animator.GetIKPosition(foot);
            Quaternion footRot = animator.GetIKRotation(foot);
            float rayDist = Settings.raycastDistance;
            Vector3 rayOrigin = footBone.position + Vector3.up * rayDist;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDist * 2f,
                    EnvironmentManager.Settings.GroundLayer))
            {
                Vector3 targetPos = hit.point + hit.normal * Settings.footHeightOffset;
                Quaternion targetRot =
                    Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);
                Vector3 smoothedPos = Vector3.Lerp(footPos, targetPos, Time.deltaTime * blendSpeed);
                Quaternion smoothedRot = Quaternion.Slerp(footRot, targetRot, Time.deltaTime * blendSpeed);

                animator.SetIKPosition(foot, smoothedPos);
                animator.SetIKRotation(foot, smoothedRot);

                Blend = smoothedPos.y;

                Transform thigh = animator.GetBoneTransform(foot == AvatarIKGoal.LeftFoot
                    ? HumanBodyBones.LeftUpperLeg
                    : HumanBodyBones.RightUpperLeg);
                Vector3 hintOffset = transform.forward * Settings.kneeHintForward + transform.right *
                    (foot == AvatarIKGoal.LeftFoot ? -Settings.kneeHintOutward : Settings.kneeHintOutward);

                animator.SetIKHintPosition(kneeHint, thigh.position + hintOffset);
            }
        }

        #endregion

        #region Event Handlers .........................................................................................

        #endregion

        #endregion
    }
}