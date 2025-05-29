using Core;
using EmberAI.Attributes;
using EmberAI.Core;
using EmberAI.Core.Util;
using EmberAI.Settings;
using EmberAI.UI;
using JetBrains.Annotations;
using UnityEngine;

namespace EmberAI.Cameras
{
    public class ThirdPersonCamera : EmberSingleton<ThirdPersonCamera>
    {
        public enum RotationMode { LeftMouseButton, RightMouseButton, Always, AutoLeft, AutoRight }

        // smoothing & zoom
        private float _targetZoomDistance, _currentZoomVelocity, _currentRotationVelocityX, _currentRotationVelocityY;
        private Vector3 _smoothPosition;

        // blocking
        private float _blockedDistance = 10f;
        private float _blockedDistanceV;

        // rotation & position targets
        private Quaternion _rotationForSpace = Quaternion.identity;
        private Quaternion _targetRotation   = Quaternion.identity;
        private Vector3   _targetPosition   = Vector3.zero;
        private Vector3   _lastUp;

        private Camera _camera;
        private float  _cachedRotationSpeed;

        [BoxGroup("Settings"), SerializeField]
        protected Transform _target;

        [BoxGroup("Settings"), SerializeField, OnValueChanged(nameof(OnChangeSettings))]
        protected ThirdPersonCameraSettings Settings;

        [BoxGroup("Debug"), SerializeField, ReadOnly]
        [UsedImplicitly] private bool LeftButtonDown, RightButtonDown;

        private float DistanceTarget { get; set; }
        public static bool InputDisabled { get; set; }
        private Vector3 MoveDirection  { get; set; }
        private float   XRotation      { get; set; }
        private float   YRotation      { get; set; }

        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        public float FollowSpeed
        {
            set => Settings.followSpeed = value;
        }

        /// <summary>Adds to any Zoom input (scroll wheel) to adjust zoom position</summary>
        public float ZoomModifier { get; set; }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360) angle += 360;
            if (angle >  360) angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }

        private Transform RotationSpace =>
            Settings.rotationSpace != null ? Settings.rotationSpace : _target;

        private void OnChangeSettings()
        {
            _cachedRotationSpeed = Settings.rotationSpeed;
        }

        public override void InitializeDependencies()
        {
            base.InitializeDependencies();
            _camera = this.GetOrAddComponent<Camera>();
        }

        protected override void OnAwake()
        {
            base.OnAwake();

            DistanceTarget      = Settings.distance;
            _targetZoomDistance = Settings.distance;
            _smoothPosition     = transform.position;

            Vector3 angles    = transform.eulerAngles;
            XRotation          = angles.y;
            YRotation          = ClampAngle(angles.x, Settings.rotationRange.x, Settings.rotationRange.y);

            // If no rotationSpace assigned, default to target or world-up
            _lastUp = (RotationSpace != null ? RotationSpace.up : Vector3.up);
            _targetRotation = Quaternion.Euler(YRotation, XRotation, 0);
            _targetPosition = transform.position;

            _cachedRotationSpeed = Settings.rotationSpeed;
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

            if (_camera == null)
                _camera = Camera.main;

            UpdateInput();

            if (Settings.updateMode == UpdateMode.LateUpdate)
                UpdateTransform(Time.deltaTime);
        }

        protected virtual void UpdateTransform(float deltaTime)
        {
            if (!_camera.enabled) return;

            CalculateTargetRotation(deltaTime);
            CalculateTargetPosition(deltaTime);

            transform.rotation = _targetRotation;
            transform.position = _targetPosition;
        }

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

        public void UpdateInput()
        {
            if (!_camera.enabled) return;
            SetRotation(GetRotationInput());
            SetMoveDirection(GetZoomInput(Time.deltaTime));
        }

        private float GetZoomInput(float deltaTime)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Input.GetKey(Settings.zoomInKey))  _targetZoomDistance -= Settings.zoomSpeed;
            else if (Input.GetKey(Settings.zoomOutKey)) _targetZoomDistance += Settings.zoomSpeed;

            if (scroll >  0) _targetZoomDistance -= Settings.zoomSpeed;
            else if (scroll < 0) _targetZoomDistance += Settings.zoomSpeed;

            _targetZoomDistance = Mathf.Clamp(_targetZoomDistance, Settings.minDistance, Settings.maxDistance);

            DistanceTarget = Mathf.SmoothDamp(
                DistanceTarget,
                _targetZoomDistance,
                ref _currentZoomVelocity,
                Settings.mouseZoomSmoothness
            );

            float zoomMovement = (DistanceTarget - Settings.distance) * deltaTime;
            Settings.distance = DistanceTarget;
            return zoomMovement;
        }

        private Vector2 GetRotationInput()
        {
            Vector2 targetRotation = Vector2.zero;

            if (Settings.rotationMode == RotationMode.AutoLeft)
            {
                targetRotation.x = -Settings.rotationSpeed * Settings.nonMouseRotationModifier;
            }
            else if (Settings.rotationMode == RotationMode.AutoRight)
            {
                targetRotation.x = Settings.rotationSpeed * Settings.nonMouseRotationModifier;
            }
            else
            {
                LeftButtonDown  = InputUtil.IsLeftMouseDown();
                RightButtonDown = InputUtil.IsRightMouseDown();

                if (Settings.rotationMode == RotationMode.Always ||
                    (Settings.rotationMode == RotationMode.LeftMouseButton  && LeftButtonDown) ||
                    (Settings.rotationMode == RotationMode.RightMouseButton && RightButtonDown))
                {
                    targetRotation = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                }

                bool isKeyRot = false;
                if (InputUtil.IsKeyDown(Settings.rotateLeftKey))  { isKeyRot = true; targetRotation.x =  1; }
                else if (InputUtil.IsKeyDown(Settings.rotateRightKey)) { isKeyRot = true; targetRotation.x = -1; }
                if (InputUtil.IsKeyDown(Settings.rotateUpKey))    { isKeyRot = true; targetRotation.y =  1; }
                else if (Input.GetKey(Settings.rotateDownKey))    { isKeyRot = true; targetRotation.y = -1; }

                targetRotation *= Settings.rotationSpeed;
                if (isKeyRot)
                    targetRotation *= Settings.nonMouseRotationModifier;
            }

            XRotation = Mathf.SmoothDamp(
                XRotation,
                XRotation + targetRotation.x,
                ref _currentRotationVelocityX,
                Settings.mouseRotateSmoothness
            );
            YRotation = Mathf.SmoothDamp(
                YRotation,
                YRotation - targetRotation.y,
                ref _currentRotationVelocityY,
                Settings.mouseRotateSmoothness
            );

            YRotation = ClampAngle(YRotation, Settings.rotationRange.x, Settings.rotationRange.y);
            return targetRotation;
        }

        private void SetRotation(Vector2 rotation)
        {
            if (rotation.x > 5 || rotation.y > 5) return;
            XRotation += rotation.x * Settings.rotationSpeed;
            YRotation  = ClampAngle(
                YRotation - rotation.y * Settings.rotationSpeed,
                Settings.rotationRange.x,
                Settings.rotationRange.y
            );
        }

        private void CalculateTargetRotation(float deltaTime)
        {
            _targetRotation =
                Quaternion.AngleAxis(XRotation, Vector3.up) *
                Quaternion.AngleAxis(YRotation, Vector3.right);

            var space = RotationSpace;
            if (space != null)
            {
                _rotationForSpace = Quaternion
                    .FromToRotation(_lastUp, space.up)
                    * _rotationForSpace;

                _targetRotation = _rotationForSpace * _targetRotation;
                _lastUp         = space.up;
            }
        }

        private void SetMoveDirection(float amount)
        {
            MoveDirection = transform.forward * amount;
        }

        protected virtual void CalculateTargetPosition(float deltaTime)
        {
            if (_target == null) return;

            DistanceTarget = Mathf.Clamp(
                DistanceTarget + MoveDirection.z,
                Settings.minDistance,
                Settings.maxDistance
            );

            Settings.distance += (DistanceTarget - Settings.distance) *
                                  Settings.zoomSpeed * deltaTime;

            _smoothPosition = Settings.smoothFollow
                ? Vector3.Lerp(
                    _smoothPosition,
                    _target.position,
                    deltaTime * Settings.followSpeed
                  )
                : _target.position;

            var t = _smoothPosition + _targetRotation * Settings.offset;
            var f = _targetRotation * -Vector3.forward;

            if (Settings.blockingLayers != -1)
            {
                if (Physics.SphereCast(
                    t - f * Settings.blockingOriginOffset,
                    Settings.blockingRadius,
                    f,
                    out var hit,
                    Settings.blockingOriginOffset + DistanceTarget - Settings.blockingRadius,
                    Settings.blockingLayers
                ))
                {
                    _blockedDistance = Mathf.SmoothDamp(
                        _blockedDistance,
                        hit.distance +
                          Settings.blockingRadius * (1f - Settings.blockedOffset) -
                          Settings.blockingOriginOffset,
                        ref _blockedDistanceV,
                        Settings.blockingSmoothTime
                    );
                }
                else
                {
                    _blockedDistance = DistanceTarget;
                }

                Settings.distance = Mathf.Min(Settings.distance, _blockedDistance);
            }

            _targetPosition = t + f * Settings.distance;
        }
    }
}
