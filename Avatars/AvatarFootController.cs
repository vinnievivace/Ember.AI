using System;
using EmberAI.Envrionment;
using JetBrains.Annotations;
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

        private bool _initialized;
        private Animator _animator;
        private IKTarget[] _targets;
        private float _bodyOffset = 0f;
        
        public float groundedControlTolerance = 0.02f;
        public Boolean advancedForwardDetect = true;
        public float advancedForwardDetectionRange = 0.25f;

        public float raycastDistance = 0.5f;

        public float fixedVerticalBodyPositionOffset = 0f;
        public float lerpValue = 0.5f;

        

        
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        public bool IsGrounded { get; private set; }
        
        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        #endregion

        #region Inspector ..............................................................................................

        #endregion

        #region Initialization .........................................................................................

        public void Initialize()
        {
            _animator = GetComponent<Animator>();
            
            _initialized = true;
        }
        
        #endregion

        #region MonoBehaviours .........................................................................................

        protected override void OnStart()
        {
            base.OnStart();
        
            _targets = new IKTarget[2];
            
            _targets[0] = new IKTarget(Vector3.zero, false, Quaternion.identity, false, AvatarIKGoal.LeftFoot,
                AvatarIKHint.LeftKnee, Vector3.zero, false, HumanBodyBones.LeftFoot, HumanBodyBones.LeftLowerLeg,
                new RaycastHit(), 0f, 0f, 0f);
            
            _targets[1] = new IKTarget(Vector3.zero, false, Quaternion.identity, false, AvatarIKGoal.RightFoot,
                AvatarIKHint.RightKnee, Vector3.zero, false, HumanBodyBones.RightFoot, HumanBodyBones.RightLowerLeg,
                new RaycastHit(), 0f, 0f, 0f);
        }

        [UsedImplicitly]
        private void OnAnimatorIK(int layerIndex)
        {
            if (!_initialized) return; 
            
            lerpValue = Mathf.Clamp01(lerpValue);
            _bodyOffset = Mathf.Lerp(_bodyOffset, CalculateTargets(), lerpValue);
            
            ApplyBodyPosition(_bodyOffset);
            
            foreach (IKTarget target in _targets)
            {
                ApplyIK(target);
            }
            
            IsGrounded = _targets[0].ikEnabled || _targets[1].ikEnabled;
        }
        
        #endregion

        #region General ................................................................................................

        private float CalculateTargets()
        {
            float maxVerticalIkDistance = 0f;
            for (int i = 0; i < _targets.Length; i++)
            {
                _targets[i].boneTargetDistance = i == 0 ? _animator.leftFeetBottomHeight : _animator.rightFeetBottomHeight;
                _targets[i].ikEnabled = false;
                _targets[i].rotationEnabled = false;
                _targets[i].hintEnabled = false;
                Vector3 oldPosition = _targets[i].position;

                if (Physics.Raycast(_animator.GetBoneTransform(_targets[i].ikBone).position + new Vector3(0, raycastDistance, 0),
                        Vector3.down, out _targets[i].hit, 2f * raycastDistance, EnvironmentManager.Settings.GroundLayer))
                {
                    float originalHitY = _targets[i].hit.point.y;
                    if (GetRootPosition().y + groundedControlTolerance >
                        (_animator.GetBoneTransform(_targets[i].ikBone).position.y) - (_targets[i].boneTargetDistance))
                    {
                        // feet position is lower than tolerated height near floor (root)
                        // Must place the feet on ground
                        _targets[i].position = _targets[i].hit.point + new Vector3(0, _targets[i].boneTargetDistance, 0);
                        _targets[i].rotation = Quaternion.FromToRotation(Vector3.up, _targets[i].hit.normal) *
                                              _animator.GetIKRotation(_targets[i].ikGoal);


                    }
                    else
                    {
                        // feet position is higher than tolerated height near floor
                        // Must place on the animation height over the floor without any rotation
                        _targets[i].position = _targets[i].hit.point + new Vector3(0,
                            _animator.GetBoneTransform(_targets[i].ikBone).position.y - GetRootPosition().y, 0);
                        _targets[i].rotation = _animator.GetIKRotation(_targets[i].ikGoal);
                    }

                    if (advancedForwardDetect)
                    {
                        Vector3 footVelocity = Vector3.zero;
                        if (oldPosition != Vector3.zero)
                        {
                            footVelocity = _targets[i].position - oldPosition;
                        }

                        float angle = 180f;
                        if (footVelocity != Vector3.zero)
                        {
                            angle = Vector3.Angle(footVelocity, _animator.velocity);
                        }

                        if (angle < 90f)
                        {
                            if (Physics.Raycast(
                                    _targets[i].position +
                                    (_animator.velocity.normalized * advancedForwardDetectionRange) +
                                    new Vector3(0, raycastDistance, 0), Vector3.down, out _targets[i].hit,
                                    2f * raycastDistance, EnvironmentManager.Settings.GroundLayer))
                            {
                                if (originalHitY < _targets[i].hit.point.y &&
                                    Vector3.Angle(_targets[i].hit.normal, Vector3.up) < 10)
                                {
                                    _targets[i].advancedForwardDetectionHeight = Mathf.Lerp(
                                        _targets[i].advancedForwardDetectionHeight,
                                        (_targets[i].hit.point.y - originalHitY), lerpValue);
                                    _targets[i].advancedForwardDetectionHeight =
                                        Mathf.Clamp01(_targets[i].advancedForwardDetectionHeight);
                                    _targets[i].position.y += _targets[i].advancedForwardDetectionHeight;
                                }
                                else
                                {
                                    _targets[i].advancedForwardDetectionHeight =
                                        Mathf.Lerp(_targets[i].advancedForwardDetectionHeight, 0f, lerpValue);
                                }
                            }
                            else
                            {
                                _targets[i].advancedForwardDetectionHeight =
                                    Mathf.Lerp(_targets[i].advancedForwardDetectionHeight, 0f, lerpValue);
                            }
                        }
                        else
                        {
                            _targets[i].advancedForwardDetectionHeight =
                                Mathf.Lerp(_targets[i].advancedForwardDetectionHeight, 0f, lerpValue);
                        }


                    }

                    _targets[i].ikEnabled = true;
                    _targets[i].rotationEnabled = true;
                    if (_animator.GetBoneTransform(_targets[i].ikBone).position.y - _targets[i].position.y >
                        maxVerticalIkDistance)
                        maxVerticalIkDistance = _animator.GetBoneTransform(_targets[i].ikBone).position.y -
                                                _targets[i].position.y;

                    _targets[i].hintOffset = Vector3.Distance(_animator.GetBoneTransform(_targets[i].ikBone).position,
                        _targets[i].position);
                    _targets[i].hintEnabled = true;
                }
            }

            return maxVerticalIkDistance;
        }

        private Vector3 GetRootPosition()
        {
            if (_animator != null)
            {
                return _animator.rootPosition;
            }
            else
            {
                return transform.position;
            }
        }

        private void ApplyBodyPosition(float maxVerticalIkDistance)
        {
            fixedVerticalBodyPositionOffset = Mathf.Clamp(fixedVerticalBodyPositionOffset, -0.05f, 0.05f);
            Vector3 bodyPositionDelta = new Vector3(0, -maxVerticalIkDistance + fixedVerticalBodyPositionOffset, 0);
            _animator.bodyPosition = _animator.bodyPosition + bodyPositionDelta;
        }

        private void ApplyIK(IKTarget target)
        {
            if (_animator == null)
                return;
            if (!target.ikEnabled)
            {
                _animator.SetIKPositionWeight(target.ikGoal, 0.0f);
                _animator.SetIKRotationWeight(target.ikGoal, 0.0f);
                _animator.SetIKHintPositionWeight(target.ikHint, 0.0f);
                return;
            }

            _animator.SetIKPosition(target.ikGoal, target.position);
            _animator.SetIKPositionWeight(target.ikGoal, 1.0f);

            if (!target.rotationEnabled)
            {
                _animator.SetIKRotationWeight(target.ikGoal, 0.0f);
            }
            else
            {
                _animator.SetIKRotation(target.ikGoal, target.rotation);
                _animator.SetIKRotationWeight(target.ikGoal, 1.0f);
            }

            if (!target.hintEnabled)
            {
                _animator.SetIKHintPositionWeight(target.ikHint, 0.0f);
                return;
            }

            Vector3 localPosition =
                this.gameObject.transform.InverseTransformPoint(_animator.GetIKHintPosition(target.ikHint));
            localPosition += new Vector3(0, target.hintOffset, target.hintOffset);
            Vector3 globalPosition = this.gameObject.transform.TransformPoint(localPosition);
            _animator.SetIKHintPosition(target.ikHint, globalPosition);
            _animator.SetIKHintPositionWeight(target.ikHint, 1.0f);

        }
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

        #endregion
    }
    
    public struct IKTarget
    {
        public Vector3 position;
        public Vector3 hintPosition;
        public Boolean ikEnabled;
        public Quaternion rotation;
        public Boolean rotationEnabled;
        public AvatarIKGoal ikGoal;
        public AvatarIKHint ikHint;
        public Boolean hintEnabled;
        public HumanBodyBones ikBone;
        public HumanBodyBones hintBone;
        public RaycastHit hit;
        public float boneTargetDistance;
        public float hintOffset;
        public float advancedForwardDetectionHeight;

        public IKTarget(Vector3 position,
            Boolean ikEnabled,
            Quaternion rotation,
            Boolean rotationEnabled,
            AvatarIKGoal ikGoal,
            AvatarIKHint ikHint,
            Vector3 hintPosition,
            Boolean hintEnabled,
            HumanBodyBones ikBone,
            HumanBodyBones hintBone,
            RaycastHit hit,
            float boneTargetDistance,
            float hintOffset,
            float advancedForwardDetectionHeight)
        {
            this.position = position;
            this.hintPosition = hintPosition;
            this.ikEnabled = ikEnabled;
            this.rotation = rotation;
            this.rotationEnabled = rotationEnabled;
            this.ikGoal = ikGoal;
            this.ikHint = ikHint;
            this.hintEnabled = hintEnabled;
            this.ikBone = ikBone;
            this.hintBone = hintBone;
            this.hit = hit;
            this.boneTargetDistance = boneTargetDistance;
            this.hintOffset = hintOffset;
            this.advancedForwardDetectionHeight = advancedForwardDetectionHeight;
        }
    }
}
