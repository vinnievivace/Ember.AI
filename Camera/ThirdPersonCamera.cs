using Core;
using EmberAI.Attributes;
using EmberAI.Core;
using EmberAI.Core.Util;
using EmberAI.Envrionment;
using EmberAI.Settings;
using EmberAI.UI;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace EmberAI.Cameras
{
    public class ThirdPersonCamera : EmberSingleton<ThirdPersonCamera>
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        public enum RotationMode { LeftMouseButton, RightMouseButton, Always, AutoLeft, AutoRight }
        
        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        // smoothing & zoom
        private float _targetZoomDistance, _currentZoomVelocity, _currentRotationVelocityX, _currentRotationVelocityY;
        private Vector3 _smoothPosition;
        private float _lastAppliedDeltaX = 0f;
        private float _lastAppliedDeltaY = 0f;
        
        // blocking & spring-arm
        private float _blockedDistance = 10f;
        private float _blockedDistanceV;
        
        // rotation & position targets
        private Quaternion _rotationForSpace = Quaternion.identity;
        private Vector3   _targetPosition   = Vector3.zero;
        private Vector3   _lastUp;
        private Vector2 _rawRotationInput = Vector2.zero;
        private float  _cachedRotationSpeed;
        private float _currentFOVVelocity;
        
        [BoxGroup("Settings"), SerializeField, OnValueChanged(nameof(OnChangeSettings))]
        protected ThirdPersonCameraSettings Settings;

        [BoxGroup("Settings"), SerializeField]
        protected Transform _target;
        
        [BoxGroup("Components"), SerializeField]
        private Camera Camera;
        
        [BoxGroup("Debug"), SerializeField, ReadOnly, Tooltip("True when a ground collision is currently detected.")]
        private bool GroundCollisionDetected;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        private float DistanceTarget { get; set; }
        public static bool InputDisabled { get; set; }
        private Vector3 MoveDirection  { get; set; }
        private float   XRotation      { get; set; }
        private float   YRotation      { get; set; }

        public Transform Target { get => _target; set => _target = value; }
        public float FollowSpeed { set => Settings.followSpeed = value; }
        public float ZoomModifier { get; set; }
        
        public bool HasInput { get; private set; }
        
        private Transform RotationSpace => Settings.rotationSpace != null ? Settings.rotationSpace : _target;
        
        public Quaternion TargetRotation { get; private set; }   = Quaternion.identity;
        
        public Vector3 TargetPosition => _target.position;
        
        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        #endregion

        #region Inspector ..............................................................................................

        private void OnChangeSettings()
        {
            _cachedRotationSpeed = Settings.rotationSpeed;
            Settings.defaultDistance = Mathf.Clamp(Settings.defaultDistance, Settings.minDistance, Settings.maxDistance);
        }
        
        #endregion

        #region Initialization .........................................................................................

        public override void EditModeInitialize()
        {
            base.EditModeInitialize();
            
            Camera = this.GetOrAddComponent<Camera>();
            Camera.fieldOfView = Settings.normalFOV;
        }
        
        #endregion

        #region MonoBehaviours .........................................................................................

        protected override void OnAwake()
        {
            base.OnAwake();

            DistanceTarget      = Settings.defaultDistance;
            _targetZoomDistance = Settings.defaultDistance;
            _smoothPosition     = transform.position;

            Vector3 angles      = transform.eulerAngles;
            XRotation           = angles.y;
            YRotation           = ClampAngle(angles.x, Settings.rotationRange.x, Settings.rotationRange.y);

            _lastUp             = (RotationSpace != null ? RotationSpace.up : Vector3.up);
            TargetRotation     = Quaternion.Euler(YRotation, XRotation, 0);
            _targetPosition     = transform.position;
            _cachedRotationSpeed= Settings.rotationSpeed;
            GroundCollisionDetected = false;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (UIManager.Instance != null) InputDisabled = UIManager.Instance.UIInteraction;
            if (InputDisabled || _target == null) return;

            Cursor.lockState = Settings.lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible   = !Settings.lockCursor;

            if (Settings.updateMode == UpdateMode.Update) UpdateTransform(Time.deltaTime);
        }

        protected override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (InputDisabled || _target == null) return;

            if (Settings.updateMode == UpdateMode.FixedUpdate)
                UpdateTransform(Time.fixedDeltaTime);
        }

        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
            if (InputDisabled || _target == null) return;

            
            UpdateInput();

            if (Settings.updateMode == UpdateMode.LateUpdate)
                UpdateTransform(Time.deltaTime);
        }
        
        #endregion

        #region General ................................................................................................

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360) angle += 360;
            if (angle >  360) angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }
        
        protected virtual void CalculateTargetPosition(float deltaTime)
        {
            if (_target == null) return;
            
            DistanceTarget = Mathf.Clamp(DistanceTarget + MoveDirection.z, Settings.minDistance, Settings.maxDistance);
            Settings.defaultDistance += (DistanceTarget - Settings.defaultDistance) * Settings.zoomSpeed * deltaTime;
            _smoothPosition = Settings.smoothFollow ? Vector3.Lerp(_smoothPosition, _target.position, deltaTime * Settings.followSpeed) : _target.position;
        }
        
        public void UpdateInput()
        {
            if (!Camera.enabled) return;

            Vector2 rotation = GetRotationInput();
            float zoom = GetZoomInput(Time.deltaTime);
            
            SetRotation(rotation);
            SetZoomPosition(zoom);
            
            HasInput = rotation.sqrMagnitude > 0 || zoom != 0;
            
        }
        
        protected virtual void UpdateTransform(float deltaTime)
        {
            // 1) Compute target rotation
            CalculateTargetRotation(deltaTime);
           
            // 2) Compute target position (smoothed follow)
            CalculateTargetPosition(deltaTime);

            // 3) Spring-arm: spherecast from target backward
            Vector3 pivot = _smoothPosition + Vector3.up * Settings.offset.y;
            Vector3 dir = (TargetRotation * -Vector3.forward);
            float desiredDist = Mathf.Clamp(DistanceTarget, Settings.minDistance, Settings.maxDistance);
            Vector3 castOrigin = pivot;
            float castDistance = desiredDist + Settings.blockingDetectionRadius;

            bool hitSomething = false;
            float hitDist = desiredDist;
            
            if(Settings.BlockingLayers == 0) Debug.LogError("No ground layers set, ground collision will not work.");
            
            if (Physics.SphereCast(castOrigin, Settings.blockingDetectionRadius, dir, out RaycastHit hitInfo, castDistance, Settings.BlockingLayers))
            {
                hitDist = hitInfo.distance - Settings.blockingDetectionRadius;
                _blockedDistance = Mathf.SmoothDamp(_blockedDistance, hitDist, ref _blockedDistanceV, 0.05f);
                hitSomething = true;
            }
            else
            {
                _blockedDistance = Mathf.SmoothDamp(_blockedDistance, desiredDist, ref _blockedDistanceV, 0.05f);
            }

            float finalDistance = Mathf.Clamp(_blockedDistance, Settings.minDistance, desiredDist);
            _targetPosition = pivot + dir * finalDistance;

            // 4) FOV adjustment if too close
            float tFOV = (hitSomething && finalDistance < desiredDist - 0.1f) ? Settings.blockedFOV : Settings.normalFOV;
            Camera.fieldOfView = Mathf.SmoothDamp(Camera.fieldOfView, tFOV, ref _currentFOVVelocity, Settings.FOVSmoothTime);

            // 5) Track ground collision for debug
            GroundCollisionDetected = hitSomething;

            // 6) Apply to transform
            transform.rotation = TargetRotation;
            transform.position = _targetPosition;
        }
        
        #endregion
        
        #region Rotation ...............................................................................................
        
        [UsedImplicitly]
        public void SetRotationMode(RotationMode mode)
        {
            if (mode == Settings.rotationMode) return;

            bool wasAuto = Settings.rotationMode is RotationMode.AutoLeft or RotationMode.AutoRight;
            if (wasAuto)
            {
                TweenUtil.TweenFloat(Settings.rotationSpeed, 0, Settings.rotationDuration,
                    v => Settings.rotationSpeed = v);
            }

            if (mode is RotationMode.AutoLeft or RotationMode.AutoRight)
            {
                CallbackManager.AddOneOff(this, wasAuto ? 1 : 0, () =>
                {
                    Settings.rotationMode = mode;
                    TweenUtil.TweenFloat(0, _cachedRotationSpeed, Settings.rotationDuration,
                        v => Settings.rotationSpeed = v);
                });
            }
            else
            {
                CallbackManager.AddOneOff(this, wasAuto ? Settings.rotationDuration : 0, () =>
                {
                    Settings.rotationMode    = mode;
                    Settings.rotationSpeed = _cachedRotationSpeed;
                });
            }
        }
        
        private Vector2 GetRotationInput()
        {
            Vector2 input = Vector2.zero;
            bool hasKeyboardInput = false;

            // Keyboard input takes priority over everything else
            if (InputUtil.IsKeyDown(Settings.rotateLeftKey))  { input.x += 1f; hasKeyboardInput = true; }
            if (InputUtil.IsKeyDown(Settings.rotateRightKey)) { input.x -= 1f; hasKeyboardInput = true; }
            if (InputUtil.IsKeyDown(Settings.rotateUpKey))    { input.y += 1f; hasKeyboardInput = true; }
            if (InputUtil.IsKeyDown(Settings.rotateDownKey))  { input.y -= 1f; hasKeyboardInput = true; }

            if (hasKeyboardInput)
            {
                input *= Settings.rotationSpeed * Settings.nonMouseRotationModifier;
            }
            else if (Settings.rotationMode == RotationMode.AutoLeft)
            {
                input.x = -Settings.rotationSpeed * Settings.nonMouseRotationModifier;
            }
            else if (Settings.rotationMode == RotationMode.AutoRight)
            {
                input.x = Settings.rotationSpeed * Settings.nonMouseRotationModifier;
            }
            else if (
                Settings.rotationMode == RotationMode.Always ||
                (Settings.rotationMode == RotationMode.LeftMouseButton  && InputUtil.IsLeftMouseDown()) ||
                (Settings.rotationMode == RotationMode.RightMouseButton && InputUtil.IsRightMouseDown()))
            {
                input = InputUtil.GetMouseAxis() * Settings.rotationSpeed;
            }

            _rawRotationInput = input;
            return input;
        }

        private void SetRotation(Vector2 _)
        {
            float smoothedDeltaX = Mathf.SmoothDamp(_lastAppliedDeltaX, _rawRotationInput.x, ref _currentRotationVelocityX, Settings.mouseRotateSmoothness);
            float smoothedDeltaY = Mathf.SmoothDamp(_lastAppliedDeltaY, _rawRotationInput.y, ref _currentRotationVelocityY, Settings.mouseRotateSmoothness);

            XRotation += smoothedDeltaX;
            YRotation = ClampAngle(YRotation - smoothedDeltaY, Settings.rotationRange.x, Settings.rotationRange.y);

            _lastAppliedDeltaX = smoothedDeltaX;
            _lastAppliedDeltaY = smoothedDeltaY;
        }



        private void CalculateTargetRotation(float deltaTime)
        {
            TargetRotation = Quaternion.AngleAxis(XRotation, Vector3.up) * Quaternion.AngleAxis(YRotation, Vector3.right);
            var space = RotationSpace;
            if (space != null)
            {
                _rotationForSpace = Quaternion.FromToRotation(_lastUp, space.up) * _rotationForSpace;
                TargetRotation = _rotationForSpace * TargetRotation;
                _lastUp = space.up;
            }
        }
        
        #endregion
        
        #region Zoom ...................................................................................................
        
        private void SetZoomPosition(float amount)
        {
            MoveDirection = transform.forward * amount;
        }
        
        private float GetZoomInput(float deltaTime)
        {
            float scroll = InputUtil.GetMouseScroll();
            
            if (InputUtil.IsKeyDown(Settings.zoomInKey))  _targetZoomDistance -= Settings.zoomSpeed;
            else if (InputUtil.IsKeyDown(Settings.zoomOutKey)) _targetZoomDistance += Settings.zoomSpeed;

            if (scroll >  0) _targetZoomDistance -= Settings.zoomSpeed;
            else if (scroll < 0) _targetZoomDistance += Settings.zoomSpeed;

            _targetZoomDistance = Mathf.Clamp(_targetZoomDistance, Settings.minDistance, Settings.maxDistance);
            DistanceTarget = Mathf.SmoothDamp(DistanceTarget, _targetZoomDistance, ref _currentZoomVelocity, Settings.mouseZoomSmoothness);
            return (_targetZoomDistance - Settings.defaultDistance) * deltaTime;
        }
        
        #endregion
        
        #region Event Handlers .........................................................................................

        #endregion

        #endregion
        
        
    }
}
