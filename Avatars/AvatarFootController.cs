using System;
using EmberAI.Attributes;
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

        private CharacterSettings _settings;
        private bool _initialized;
        private Animator _animator;
        private IKTarget _leftFootIKTarget;
        private IKTarget _rightFootIKTarget;
        private float _bodyOffset;
        
        [BoxGroup("Components"), ReadOnly, SerializeField]
        private Transform LeftFoot, RightFoot;
        
        [BoxGroup("Debug"), Tooltip("Show debug gizmos for IK targets, raycasts, and grounded state.")]
        public bool showGizmos = true;
        
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

        public override void EditModeInitialize()
        {
            base.EditModeInitialize();
            
            description = "Handles foot IK and grounded state for the avatar.";
        }

        public void Initialize(CharacterSettings settings)
        {
            _animator = GetComponent<Animator>();
            _settings = settings;
            LeftFoot = _animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            RightFoot = _animator.GetBoneTransform(HumanBodyBones.RightFoot);
            _initialized = true;
        }
        
        #endregion

        #region MonoBehaviours .........................................................................................

        protected override void OnStart()
        {
            base.OnStart();
        
            _leftFootIKTarget = new IKTarget(Vector3.zero, false, Quaternion.identity, false, AvatarIKGoal.LeftFoot,
                AvatarIKHint.LeftKnee, HumanBodyBones.LeftFoot, new RaycastHit(), 0f, 0f, 0f);
            
            _rightFootIKTarget = new IKTarget(Vector3.zero, false, Quaternion.identity, false, AvatarIKGoal.RightFoot,
                AvatarIKHint.RightKnee, HumanBodyBones.RightFoot, new RaycastHit(), 0f, 0f, 0f);
        }

        [UsedImplicitly]
        private void OnAnimatorIK(int layerIndex)
        {
            if (!_initialized || _animator == null || _settings == null) return;
            
            _settings.ikSmoothing = Mathf.Clamp01(_settings.ikSmoothing);
            _bodyOffset = Mathf.Lerp(_bodyOffset, CalculateTargets(), _settings.ikSmoothing);
            
            ApplyBodyPosition(_bodyOffset);
            
            ApplyIK(_leftFootIKTarget);
            ApplyIK(_rightFootIKTarget);
            
            IsGrounded = _leftFootIKTarget.ikEnabled || _rightFootIKTarget.ikEnabled;
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showGizmos || !_initialized || _animator == null) return;

            // Draw for each foot
            DrawIKTargetGizmos(_leftFootIKTarget, "LeftFoot");
            DrawIKTargetGizmos(_rightFootIKTarget, "RightFoot");
            // Draw body position sphere (green if either foot grounded, red if not)
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawSphere(_animator.bodyPosition, 0.04f);
            UnityEditor.Handles.Label(_animator.bodyPosition + Vector3.up * 0.08f, $"Body\nGrounded: {IsGrounded}");
        }

        private void DrawIKTargetGizmos(IKTarget target, string footName)
        {
            // Raycast origin and direction
            Transform bone = _animator.GetBoneTransform(target.ikBone);
            if (bone == null) return;
            Vector3 rayOrigin = bone.position + Vector3.up * _settings.footRaycastDistance;
            Vector3 rayDir = Vector3.down * _settings.footRaycastDistance * 2f;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(rayOrigin, rayOrigin + rayDir);
            // Draw hit point
            if (target.hit.collider != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(target.hit.point, 0.02f);
                Gizmos.DrawLine(target.hit.point, target.hit.point + target.hit.normal * 0.2f);
            }
            // Draw IK target position
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(target.position, 0.025f);
            // Label for grounded state
            string label = $"{footName}\nGrounded: {target.ikEnabled}";
            UnityEditor.Handles.Label(target.position + Vector3.up * 0.05f, label);
        }
        #endif
        
        #endregion

        #region General ...............................................................................................

        /// <summary>
        /// Calculates and updates IK targets for both feet by raycasting to the ground,
        /// determining if each foot should be grounded, and applying advanced forward detection if enabled.
        /// Returns the maximum vertical IK adjustment needed for the body offset.
        /// </summary>
        /// <returns>The maximum vertical distance any foot needs to be adjusted by IK.</returns>
        private float CalculateTargets()
        {
            float maxVerticalIkDistance = 0f;

            // Left foot
            _leftFootIKTarget.boneTargetDistance = _animator.leftFeetBottomHeight;
            _leftFootIKTarget.ikEnabled = false;
            _leftFootIKTarget.rotationEnabled = false;
            _leftFootIKTarget.hintEnabled = false;
            Vector3 oldLeftPosition = _leftFootIKTarget.position;

            Vector3 leftRayOrigin = _animator.GetBoneTransform(_leftFootIKTarget.ikBone).position + new Vector3(0, _settings.footRaycastDistance, 0);
            if (Physics.Raycast(leftRayOrigin, Vector3.down, out _leftFootIKTarget.hit, 2f * _settings.footRaycastDistance, EnvironmentManager.Settings.GroundLayer))
            {
                float originalLeftHitY = _leftFootIKTarget.hit.point.y;

                // Root-relative grounding: is the foot below the root (body) height + tolerance?
                if (GetRootPosition().y + _settings.groundedTolerance >
                    _animator.GetBoneTransform(_leftFootIKTarget.ikBone).position.y - _leftFootIKTarget.boneTargetDistance)
                {
                    // Place the foot on the ground, aligned to the surface normal
                    _leftFootIKTarget.position = _leftFootIKTarget.hit.point + new Vector3(0, _leftFootIKTarget.boneTargetDistance, 0);
                    _leftFootIKTarget.rotation = Quaternion.FromToRotation(Vector3.up, _leftFootIKTarget.hit.normal) * _animator.GetIKRotation(_leftFootIKTarget.ikGoal);
                }
                else
                {
                    // Foot is too high: use the animated height above the ground, no rotation
                    _leftFootIKTarget.position = _leftFootIKTarget.hit.point + new Vector3(0, _animator.GetBoneTransform(_leftFootIKTarget.ikBone).position.y - GetRootPosition().y, 0);
                    _leftFootIKTarget.rotation = _animator.GetIKRotation(_leftFootIKTarget.ikGoal);
                }

                // Advanced forward detection: look ahead for steps/bumps if enabled
                if (_settings.enableForwardStepDetection)
                {
                    Vector3 leftFootVelocity = oldLeftPosition != Vector3.zero ? _leftFootIKTarget.position - oldLeftPosition : Vector3.zero;
                    float leftAngle = (leftFootVelocity != Vector3.zero) ? Vector3.Angle(leftFootVelocity, _animator.velocity) : 180f;

                    if (leftAngle < 90f)
                    {
                        // Look ahead in the direction of movement for a higher step
                        Vector3 leftForwardRayOrigin = _leftFootIKTarget.position + (_animator.velocity.normalized * _settings.forwardStepDetectionRange) + new Vector3(0, _settings.footRaycastDistance, 0);
                        if (Physics.Raycast(leftForwardRayOrigin, Vector3.down, out _leftFootIKTarget.hit, 2f * _settings.footRaycastDistance, EnvironmentManager.Settings.GroundLayer))
                        {
                            if (originalLeftHitY < _leftFootIKTarget.hit.point.y && Vector3.Angle(_leftFootIKTarget.hit.normal, Vector3.up) < 10)
                            {
                                // Smoothly adjust the foot height to match the upcoming step
                                _leftFootIKTarget.advancedForwardDetectionHeight = Mathf.Lerp(
                                    _leftFootIKTarget.advancedForwardDetectionHeight,
                                    (_leftFootIKTarget.hit.point.y - originalLeftHitY), _settings.ikSmoothing);
                                _leftFootIKTarget.advancedForwardDetectionHeight = Mathf.Clamp01(_leftFootIKTarget.advancedForwardDetectionHeight);
                                _leftFootIKTarget.position.y += _leftFootIKTarget.advancedForwardDetectionHeight;
                            }
                            else
                            {
                                // No step detected, smoothly reset the adjustment
                                _leftFootIKTarget.advancedForwardDetectionHeight = Mathf.Lerp(_leftFootIKTarget.advancedForwardDetectionHeight, 0f, _settings.ikSmoothing);
                            }
                        }
                        else
                        {
                            _leftFootIKTarget.advancedForwardDetectionHeight = Mathf.Lerp(_leftFootIKTarget.advancedForwardDetectionHeight, 0f, _settings.ikSmoothing);
                        }
                    }
                    else
                    {
                        _leftFootIKTarget.advancedForwardDetectionHeight = Mathf.Lerp(_leftFootIKTarget.advancedForwardDetectionHeight, 0f, _settings.ikSmoothing);
                    }
                }

                // Enable IK for this foot
                _leftFootIKTarget.ikEnabled = true;
                _leftFootIKTarget.rotationEnabled = true;

                // Track the maximum vertical IK adjustment for pelvis/body offset
                float leftVerticalIkDistance = _animator.GetBoneTransform(_leftFootIKTarget.ikBone).position.y - _leftFootIKTarget.position.y;
                if (leftVerticalIkDistance > maxVerticalIkDistance)
                    maxVerticalIkDistance = leftVerticalIkDistance;

                // Calculate hint offset for knee direction
                _leftFootIKTarget.hintOffset = Vector3.Distance(_animator.GetBoneTransform(_leftFootIKTarget.ikBone).position, _leftFootIKTarget.position);
                _leftFootIKTarget.hintEnabled = true;
            }

            // Right foot
            _rightFootIKTarget.boneTargetDistance = _animator.rightFeetBottomHeight;
            _rightFootIKTarget.ikEnabled = false;
            _rightFootIKTarget.rotationEnabled = false;
            _rightFootIKTarget.hintEnabled = false;
            Vector3 oldRightPosition = _rightFootIKTarget.position;

            Vector3 rightRayOrigin = _animator.GetBoneTransform(_rightFootIKTarget.ikBone).position + new Vector3(0, _settings.footRaycastDistance, 0);
            if (Physics.Raycast(rightRayOrigin, Vector3.down, out _rightFootIKTarget.hit, 2f * _settings.footRaycastDistance, EnvironmentManager.Settings.GroundLayer))
            {
                float originalRightHitY = _rightFootIKTarget.hit.point.y;

                // Root-relative grounding: is the foot below the root (body) height + tolerance?
                if (GetRootPosition().y + _settings.groundedTolerance >
                    _animator.GetBoneTransform(_rightFootIKTarget.ikBone).position.y - _rightFootIKTarget.boneTargetDistance)
                {
                    // Place the foot on the ground, aligned to the surface normal
                    _rightFootIKTarget.position = _rightFootIKTarget.hit.point + new Vector3(0, _rightFootIKTarget.boneTargetDistance, 0);
                    _rightFootIKTarget.rotation = Quaternion.FromToRotation(Vector3.up, _rightFootIKTarget.hit.normal) * _animator.GetIKRotation(_rightFootIKTarget.ikGoal);
                }
                else
                {
                    // Foot is too high: use the animated height above the ground, no rotation
                    _rightFootIKTarget.position = _rightFootIKTarget.hit.point + new Vector3(0, _animator.GetBoneTransform(_rightFootIKTarget.ikBone).position.y - GetRootPosition().y, 0);
                    _rightFootIKTarget.rotation = _animator.GetIKRotation(_rightFootIKTarget.ikGoal);
                }

                // Advanced forward detection: look ahead for steps/bumps if enabled
                if (_settings.enableForwardStepDetection)
                {
                    Vector3 rightFootVelocity = oldRightPosition != Vector3.zero ? _rightFootIKTarget.position - oldRightPosition : Vector3.zero;
                    float rightAngle = (rightFootVelocity != Vector3.zero) ? Vector3.Angle(rightFootVelocity, _animator.velocity) : 180f;

                    if (rightAngle < 90f)
                    {
                        // Look ahead in the direction of movement for a higher step
                        Vector3 rightForwardRayOrigin = _rightFootIKTarget.position + (_animator.velocity.normalized * _settings.forwardStepDetectionRange) + new Vector3(0, _settings.footRaycastDistance, 0);
                        if (Physics.Raycast(rightForwardRayOrigin, Vector3.down, out _rightFootIKTarget.hit, 2f * _settings.footRaycastDistance, EnvironmentManager.Settings.GroundLayer))
                        {
                            if (originalRightHitY < _rightFootIKTarget.hit.point.y && Vector3.Angle(_rightFootIKTarget.hit.normal, Vector3.up) < 10)
                            {
                                // Smoothly adjust the foot height to match the upcoming step
                                _rightFootIKTarget.advancedForwardDetectionHeight = Mathf.Lerp(
                                    _rightFootIKTarget.advancedForwardDetectionHeight,
                                    (_rightFootIKTarget.hit.point.y - originalRightHitY), _settings.ikSmoothing);
                                _rightFootIKTarget.advancedForwardDetectionHeight = Mathf.Clamp01(_rightFootIKTarget.advancedForwardDetectionHeight);
                                _rightFootIKTarget.position.y += _rightFootIKTarget.advancedForwardDetectionHeight;
                            }
                            else
                            {
                                // No step detected, smoothly reset the adjustment
                                _rightFootIKTarget.advancedForwardDetectionHeight = Mathf.Lerp(_rightFootIKTarget.advancedForwardDetectionHeight, 0f, _settings.ikSmoothing);
                            }
                        }
                        else
                        {
                            _rightFootIKTarget.advancedForwardDetectionHeight = Mathf.Lerp(_rightFootIKTarget.advancedForwardDetectionHeight, 0f, _settings.ikSmoothing);
                        }
                    }
                    else
                    {
                        _rightFootIKTarget.advancedForwardDetectionHeight = Mathf.Lerp(_rightFootIKTarget.advancedForwardDetectionHeight, 0f, _settings.ikSmoothing);
                    }
                }

                // Enable IK for this foot
                _rightFootIKTarget.ikEnabled = true;
                _rightFootIKTarget.rotationEnabled = true;

                // Track the maximum vertical IK adjustment for pelvis/body offset
                float rightVerticalIkDistance = _animator.GetBoneTransform(_rightFootIKTarget.ikBone).position.y - _rightFootIKTarget.position.y;
                if (rightVerticalIkDistance > maxVerticalIkDistance)
                    maxVerticalIkDistance = rightVerticalIkDistance;

                // Calculate hint offset for knee direction
                _rightFootIKTarget.hintOffset = Vector3.Distance(_animator.GetBoneTransform(_rightFootIKTarget.ikBone).position, _rightFootIKTarget.position);
                _rightFootIKTarget.hintEnabled = true;
            }

            return maxVerticalIkDistance;
        }

        /// <summary>
        /// Returns the root position of the avatar for grounding calculations.
        /// Uses the animator's root position if available, otherwise falls back to the transform's position.
        /// </summary>
        private Vector3 GetRootPosition()
        {
            return _animator != null ? _animator.rootPosition : transform.position;
        }

        /// <summary>
        /// Vertically offsets the body (pelvis) based on the maximum IK adjustment needed by the feet,
        /// plus any additional user-defined offset. This keeps the character's hips/pelvis at a natural height
        /// when feet are on uneven ground or steps.
        /// </summary>
        /// <param name="maxVerticalIkDistance">The largest vertical IK adjustment needed by any foot.</param>
        private void ApplyBodyPosition(float maxVerticalIkDistance)
        {
            // Clamp the user-defined offset to a safe range
            _settings.bodyHeightOffset = Mathf.Clamp(_settings.bodyHeightOffset, -0.05f, 0.05f);

            // Offset the body by the max foot drop plus the user offset
            Vector3 bodyPositionDelta = new Vector3(0, -maxVerticalIkDistance + _settings.bodyHeightOffset, 0);

            // Apply the offset to the animator's body position
            _animator.bodyPosition = _animator.bodyPosition + bodyPositionDelta;
        }

        /// <summary>
        /// Applies IK position, rotation, and hint to the specified foot target.
        /// Disables IK if not enabled for this foot.
        /// </summary>
        /// <param name="target">The IK target for the foot.</param>
        private void ApplyIK(IKTarget target)
        {
            if (!target.ikEnabled)
            {
                // Disable all IK for this foot
                _animator.SetIKPositionWeight(target.ikGoal, 0.0f);
                _animator.SetIKRotationWeight(target.ikGoal, 0.0f);
                _animator.SetIKHintPositionWeight(target.ikHint, 0.0f);
                return;
            }

            // Set IK position and weight
            _animator.SetIKPosition(target.ikGoal, target.position);
            _animator.SetIKPositionWeight(target.ikGoal, 1.0f);

            // Set IK rotation and weight if enabled
            if (target.rotationEnabled)
            {
                _animator.SetIKRotation(target.ikGoal, target.rotation);
                _animator.SetIKRotationWeight(target.ikGoal, 1.0f);
            }
            else
            {
                _animator.SetIKRotationWeight(target.ikGoal, 0.0f);
            }

            // Set IK hint (for knee direction) if enabled
            if (target.hintEnabled)
            {
                Vector3 localPosition = gameObject.transform.InverseTransformPoint(_animator.GetIKHintPosition(target.ikHint));
                localPosition += new Vector3(0, target.hintOffset, target.hintOffset);
                Vector3 globalPosition = this.gameObject.transform.TransformPoint(localPosition);
                _animator.SetIKHintPosition(target.ikHint, globalPosition);
                _animator.SetIKHintPositionWeight(target.ikHint, 1.0f);
            }
            else
            {
                _animator.SetIKHintPositionWeight(target.ikHint, 0.0f);
            }
        }
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

        #endregion


    }
    
    /// <summary>
    /// Represents an IK target for foot inverse kinematics, containing position, rotation,
    /// and various IK settings for a single foot.
    /// </summary>
    public struct IKTarget
    {
        /// <summary>The target position for the foot IK.</summary>
        public Vector3 position;
        
        /// <summary>Whether IK is enabled for this foot.</summary>
        public Boolean ikEnabled;
        
        /// <summary>The target rotation for the foot IK.</summary>
        public Quaternion rotation;
        
        /// <summary>Whether rotation IK is enabled for this foot.</summary>
        public Boolean rotationEnabled;
        
        /// <summary>The IK goal (LeftFoot or RightFoot).</summary>
        public AvatarIKGoal ikGoal;
        
        /// <summary>The IK hint for knee direction (LeftKnee or RightKnee).</summary>
        public AvatarIKHint ikHint;
        
        /// <summary>Whether hint IK is enabled for this foot.</summary>
        public Boolean hintEnabled;
        
        /// <summary>The foot bone (LeftFoot or RightFoot).</summary>
        public HumanBodyBones ikBone;
        
        /// <summary>Ground raycast hit data.</summary>
        public RaycastHit hit;
        
        /// <summary>Distance from foot bone to bottom of foot for correct placement.</summary>
        public float boneTargetDistance;
        
        /// <summary>Offset for knee direction hint calculation.</summary>
        public float hintOffset;
        
        /// <summary>Height adjustment for forward step detection.</summary>
        public float advancedForwardDetectionHeight;

        /// <summary>
        /// Creates a new IK target with the specified parameters.
        /// </summary>
        /// <param name="position">Target position for the foot.</param>
        /// <param name="ikEnabled">Whether IK is enabled.</param>
        /// <param name="rotation">Target rotation for the foot.</param>
        /// <param name="rotationEnabled">Whether rotation IK is enabled.</param>
        /// <param name="ikGoal">The IK goal (LeftFoot or RightFoot).</param>
        /// <param name="ikHint">The IK hint for knee direction.</param>
        /// <param name="ikBone">The foot bone.</param>
        /// <param name="hit">Ground raycast hit data.</param>
        /// <param name="boneTargetDistance">Distance from foot bone to bottom.</param>
        /// <param name="hintOffset">Offset for knee direction.</param>
        /// <param name="advancedForwardDetectionHeight">Step detection height adjustment.</param>
        public IKTarget(Vector3 position,
            Boolean ikEnabled,
            Quaternion rotation,
            Boolean rotationEnabled,
            AvatarIKGoal ikGoal,
            AvatarIKHint ikHint,
            HumanBodyBones ikBone,
            RaycastHit hit,
            float boneTargetDistance,
            float hintOffset,
            float advancedForwardDetectionHeight)
        {
            this.position = position;
            this.ikEnabled = ikEnabled;
            this.rotation = rotation;
            this.rotationEnabled = rotationEnabled;
            this.ikGoal = ikGoal;
            this.ikHint = ikHint;
            this.hintEnabled = false; // Will be set during calculation
            this.ikBone = ikBone;
            this.hit = hit;
            this.boneTargetDistance = boneTargetDistance;
            this.hintOffset = hintOffset;
            this.advancedForwardDetectionHeight = advancedForwardDetectionHeight;
        }
    }
}
