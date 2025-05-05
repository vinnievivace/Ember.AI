
using Core;
using EmberAI.Attributes;
using EmberAI.Core;
using EmberAI.Core.Util;
using EmberAI.Settings;
using EmberAI.Util;
using UnityEngine;

namespace EmberAI.Cameras
{
    public class ThirdPersonCamera : EmberSingleton<ThirdPersonCamera>
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////
        
        public enum UpdateMode { Update, FixedUpdate, LateUpdate, FixedLateUpdate }

        public enum RotationMode { LeftMouseButton, RightMouseButton, Always, AutoLeft, AutoRight }
        
        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        // these floats are used for the smoothing of rotation/zoom when using mouse, applied in GetMovementInput and GetZoomInput
        private float _targetZoomDistance, _currentZoomVelocity, _currentRotationVelocityX, _currentRotationVelocityY;
        
        private Vector3 _targetDistance, _smoothPosition;
        private float _blockedDistance = 10f, _blockedDistanceV;
        private Quaternion _rotationForSpace = Quaternion.identity;
        private bool _fixedFrame;
        private float _fixedDeltaTime;
        private Vector3 _lastUp;
        
        protected Quaternion _targetRotation = Quaternion.identity;
        protected Vector3 _targetPosition = Vector3.zero;
        
        public static bool InputDisabled { get; set; }
        
        
        [BoxGroup("Target"), SerializeField]
        protected Transform _target;
        
        private Camera _camera;

        private float _cachedRotationSpeed;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        private float DistanceTarget { get; set; } // Get/set distance
        
        protected Vector3 MoveDirection { get; private set; }
        
        protected float XRotation { get; set; } 
        protected float YRotation { get; set; }

        [BoxGroup("Settings"), SerializeField] 
        [OnValueChanged(nameof(OnChangeSettings))]
        protected ThirdPersonCameraSettings Settings;

        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        public float FollowSpeed
        {
            set => Settings.followSpeed = value; 
        }
        
        /// <summary>
        /// Adds to any Zoom input (scroll Wheel) to adjust the camera zoom position
        /// </summary>
        public float ZoomModifier { get; set; }

        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        // Clamping Euler angles
        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360) angle += 360;
            if (angle > 360) angle -= 360;
            
            return Mathf.Clamp(angle, min, max);
        }
        
        #endregion

        #region Inspector ..............................................................................................

        private void OnChangeSettings()
        {
            _cachedRotationSpeed = Settings.rotationSpeed;
        }
        
        #endregion

        #region Initialization .........................................................................................

        public override void InitializeDependencies()
        {
            base.InitializeDependencies();

            _camera = this.GetOrAddComponent<Camera>();
        }
        
        #endregion

        #region MonoBehaviours .........................................................................................
    
        protected override void OnAwake()
        {
            base.OnAwake();
    
            DistanceTarget = Settings.distance;          // Set the initial target distance to Settings.distance
            _targetZoomDistance = Settings.distance;     // Set the initial zoom distance to Settings.distance
    
            _smoothPosition = transform.position;

            Vector3 angles = transform.eulerAngles;
            XRotation = angles.y;  // Set XRotation to yaw to avoid upside down orientation
            YRotation = ClampAngle(angles.x, Settings.rotationRange.x, Settings.rotationRange.y);  // Clamp the initial pitch to stay within allowed range

            _lastUp = Settings.rotationSpace != null ? Settings.rotationSpace.up : Vector3.up;

            _targetRotation = Quaternion.Euler(YRotation, XRotation, 0);  // Update the target rotation with corrected values
            _targetPosition = transform.position;

            _cachedRotationSpeed = Settings.rotationSpeed;
        }


        protected override void OnUpdate()
        {
            base.OnUpdate();
            
            if (InputDisabled) return;
            
            if (_target == null) return;

            Cursor.lockState = Settings.lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !Settings.lockCursor;
            
            if (Settings.updateMode == UpdateMode.Update) UpdateTransform(Time.deltaTime);
        }

        protected override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            
            if (_target == null) return;
            
            if (InputDisabled) return;
            
            _fixedFrame = true;
            _fixedDeltaTime += Time.deltaTime;
            
            if (Settings.updateMode == UpdateMode.FixedUpdate) UpdateTransform(Time.deltaTime);
        }
        
        protected override void OnLateUpdate()
        {
            base.OnUpdate();
            
            if (_target == null) return;
            
            if (InputDisabled) return;
            
            if(_camera == null) _camera = Camera.main.GetComponent<Camera>();
    
            UpdateInput();

            if (Settings.updateMode == UpdateMode.LateUpdate) UpdateTransform(Time.deltaTime);

            if (Settings.updateMode == UpdateMode.FixedLateUpdate && _fixedFrame)
            {
                UpdateTransform(_fixedDeltaTime);
                _fixedDeltaTime = 0f;
                _fixedFrame = false;
            }
        }

        #endregion

        #region General ................................................................................................

        /// <summary>
        /// Applies updates to position and rotation based on inputs
        /// </summary>
        /// <param name="deltaTime"></param>
        protected virtual void UpdateTransform(float deltaTime)
        {
            if (!_camera.enabled) return;
            
            CalculateTargetRotation(deltaTime);
            CalculateTargetPosition(deltaTime);
        
            transform.rotation = _targetRotation;
            transform.position = _targetPosition;
        }
        
        public void SetAngles(Quaternion rotation)
        {
            Vector3 euler = rotation.eulerAngles;
            XRotation = euler.y;
            YRotation = euler.x;
        }

        public void SetAngles(float yaw, float pitch)
        {
            XRotation = yaw;
            YRotation = pitch;
        }

        public void ForceRotation(Vector3 rotation)
        {
            SetRotation(Vector2.zero);
            
            XRotation = rotation.x;
            YRotation = rotation.y;
        }

        public void ForcePosition(Vector3 position)
        {
            transform.position = position;
            
            MoveDirection = Vector3.zero;
        }

        public void SetRotationMode(RotationMode mode)
        {
            if (mode == Settings.rotationMode) return;
            
            bool isTweening = false;
                
            if (Settings.rotationMode == RotationMode.AutoLeft || Settings.rotationMode == RotationMode.AutoRight)
            {
                isTweening = true;
                
                TweenUtil.TweenFloat(Settings.rotationSpeed, 0, Settings.rotationDuration, (value) =>
                {
                    Settings.rotationSpeed = value;
                });
            }
            
            if (mode == RotationMode.AutoLeft || mode == RotationMode.AutoRight)
            {
                CallbackManager.AddOneOff(this, isTweening ? 1 : 0, () =>
                {
                    Settings.rotationMode = mode;
                    
                    TweenUtil.TweenFloat(0, _cachedRotationSpeed, Settings.rotationDuration, (value) =>
                    {
                        Settings.rotationSpeed = value;
                    });
                });
            }
            else
            {
                CallbackManager.AddOneOff(this, isTweening ? Settings.rotationDuration : 0, () =>
                {
                    Settings.rotationMode = mode;
                    Settings.rotationSpeed = _cachedRotationSpeed;
                });
            }
        }

        #region Input Handling .....................................................................................
        
        public void UpdateInput()
        {
            if (!_camera.enabled) return;

            SetRotation(GetRotationInput());
            SetMoveDirection(GetZoomInput(Time.deltaTime));
        }
        
        private float GetZoomInput(float deltaTime)
        {
            float scrollAxis = Input.GetAxis("Mouse ScrollWheel");
            float zoomMovement = 0;

            // Update target zoom distance based on keyboard input
            if (Input.GetKey(Settings.zoomInKey))
            {
                _targetZoomDistance -= Settings.zoomSpeed;
            }
            else if (Input.GetKey(Settings.zoomOutKey))
            {
                _targetZoomDistance += Settings.zoomSpeed;
            }

            // Update target zoom distance based on mouse scroll input
            if (scrollAxis > 0)
            {
                _targetZoomDistance -= Settings.zoomSpeed;
            }
            else if (scrollAxis < 0)
            {
                _targetZoomDistance += Settings.zoomSpeed;
            }

            // Clamp target zoom distance within min and max bounds
            _targetZoomDistance = Mathf.Clamp(_targetZoomDistance, Settings.minDistance, Settings.maxDistance);

            // Smoothly adjust the current zoom distance towards the target using SmoothDamp
            DistanceTarget = Mathf.SmoothDamp(DistanceTarget, _targetZoomDistance, ref _currentZoomVelocity, Settings.mouseZoomSmoothness);

            // Use the difference in DistanceTarget to drive movement along the camera's forward direction
            zoomMovement = DistanceTarget - Settings.distance;

            // Update the settings distance to reflect the new target distance
            Settings.distance = DistanceTarget;

            return zoomMovement * deltaTime;
        }

        private Vector2 GetRotationInput()
        {
            Vector2 targetRotation = Vector2.zero;

            // Handle rotation based on mouse input
            if (Settings.rotationMode == RotationMode.Always ||
                (Settings.rotationMode == RotationMode.LeftMouseButton && InputUtil.IsLeftMouseDown()) ||
                (Settings.rotationMode == RotationMode.RightMouseButton && InputUtil.IsRightMouseDown()))
            {
                //targetRotation = InputUtil.GetMouseAxis();
                targetRotation = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            }

            // Handle rotation based on keyboard input
            bool isKeyboardRotation = false;

            if (InputUtil.GetKey(Settings.rotateLeftKey))
            {
                isKeyboardRotation = true;
                targetRotation.x = 1;
            }
            else if (InputUtil.GetKey(Settings.rotateRightKey))
            {
                isKeyboardRotation = true;
                targetRotation.x = -1;
            }
            if (InputUtil.GetKey(Settings.rotateUpKey))
            {
                isKeyboardRotation = true;
                targetRotation.y = 1;
            }
            else if (Input.GetKey(Settings.rotateDownKey))
            {
                isKeyboardRotation = true;
                targetRotation.y = -1;
            }

            targetRotation *= Settings.rotationSpeed;

            if (isKeyboardRotation) targetRotation *= Settings.keyboardRotationModifier;

            // Smoothly adjust the current rotation towards the target using SmoothDamp
            XRotation = Mathf.SmoothDamp(XRotation, XRotation + targetRotation.x, ref _currentRotationVelocityX, Settings.mouseRotateSmoothness);
            YRotation = Mathf.SmoothDamp(YRotation, YRotation - targetRotation.y, ref _currentRotationVelocityY, Settings.mouseRotateSmoothness);

            // Clamp the Y rotation angle to prevent excessive vertical rotation
            YRotation = ClampAngle(YRotation, Settings.rotationRange.x, Settings.rotationRange.y);

            return targetRotation;
        }

        
        #endregion
        
        #region Rotation ...........................................................................................
        
        private void SetRotation(Vector2 rotation)
        {
            if (rotation.x > 5 || rotation.y > 5) return;
            
            XRotation += rotation.x * Settings.rotationSpeed;
            YRotation = ClampAngle(YRotation - rotation.y * Settings.rotationSpeed, Settings.rotationRange.x, Settings.rotationRange.y);
        }

        private void CalculateTargetRotation(float deltaTime)
        {
            _targetRotation = Quaternion.AngleAxis(XRotation, Vector3.up) * Quaternion.AngleAxis(YRotation, Vector3.right);

            if (Settings.rotationSpace != null)
            {
                _rotationForSpace = Quaternion.FromToRotation(_lastUp, Settings.rotationSpace.up) * _rotationForSpace;
                _targetRotation = _rotationForSpace * _targetRotation;

                _lastUp = Settings.rotationSpace.up;
            }
        }
        
        #endregion
        
        #region Position ...........................................................................................
        
        private void SetMoveDirection(float amount)
        {
            MoveDirection = transform.forward * amount;
        }

        protected virtual void CalculateTargetPosition(float deltaTime)
        {
            if (_target != null)
            {
                DistanceTarget = Mathf.Clamp(DistanceTarget + MoveDirection.z, Settings.minDistance, Settings.maxDistance);
                
                Settings.distance += (DistanceTarget - Settings.distance) * Settings.zoomSpeed * deltaTime;

                if (!Settings.smoothFollow) _smoothPosition = _target.position;
                else _smoothPosition = Vector3.Lerp(_smoothPosition, _target.position, deltaTime * Settings.followSpeed);

                Vector3 t = _smoothPosition + _targetRotation * Settings.offset;
                Vector3 f = _targetRotation * -Vector3.forward;

                if (Settings.blockingLayers != -1)
                {
                    RaycastHit hit;
                    if (Physics.SphereCast(t - f * Settings.blockingOriginOffset, Settings.blockingRadius, f, out hit,
                        Settings.blockingOriginOffset + DistanceTarget - Settings.blockingRadius, Settings.blockingLayers))
                    {
                        _blockedDistance = Mathf.SmoothDamp(_blockedDistance, hit.distance + Settings.blockingRadius *
                            (1f - Settings.blockedOffset) - Settings.blockingOriginOffset, ref _blockedDistanceV, Settings.blockingSmoothTime);
                    }
                    else _blockedDistance = DistanceTarget;

                    Settings.distance = Mathf.Min(Settings.distance, _blockedDistance);
                }

                _targetPosition = t + f * Settings.distance;

                transform.position = _targetPosition;
            }
        }
        
        #endregion

        #endregion
        
        #endregion
    }
}