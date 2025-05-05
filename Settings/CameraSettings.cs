using Core;
using EmberAI.Attributes;
using EmberAI.Cameras;
using EmberAI.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace EmberAI.Settings
{
    [CreateAssetMenu(fileName = "ThirdPersonCameraSettings", menuName = EmberAISystem.MenuPath +"/Settings/Camera/ThirdPersonCameraSettings")]
    public class ThirdPersonCameraSettings : BaseData
    {
	    [BoxGroup("General")]
	    public ThirdPersonCamera.UpdateMode updateMode = ThirdPersonCamera.UpdateMode.LateUpdate; 
        
	    [BoxGroup("Mouse")]
	    [Tooltip("If true, the mouse will be locked to screen center and hidden")]
	    public bool lockCursor = true;
        
	    [FormerlySerializedAs("mouseScrollSmoothness")]
	    [BoxGroup("Mouse")]
	    [Range(0,1)]
	    [Tooltip("Apply smoothing to Zoom when using scroll wheel")]
	    public float mouseZoomSmoothness = 0.5f;
	    
	    [BoxGroup("Mouse")]
	    [Range(0,1)]
	    [Tooltip("Apply smoothing to Zoom when using scroll wheel")]
	    public float mouseRotateSmoothness = 0.5f;
	    
	    [BoxGroup("Keyboard Controls")]
	    public KeyCode moveForwardKey = KeyCode.KeypadPlus, moveBackKey = KeyCode.KeypadMinus, 
		    rotateLeftKey = KeyCode.Keypad6, rotateRightKey = KeyCode.Keypad4, rotateUpKey = KeyCode.Keypad8, rotateDownKey = KeyCode.Keypad2, zoomInKey = KeyCode.KeypadPlus, zoomOutKey = KeyCode.KeypadMinus;

	    [BoxGroup("Keyboard Controls")] 
	    [Range(0,1)]
	    public float keyboardRotationModifier = 1;
	    
	    [BoxGroup("Position")]
	    [Tooltip("The offset from target relative to camera rotation")]
	    public Vector3 offset = new Vector3(0, 1.5f, 0.5f);
		
	    [BoxGroup("Position")]
	    [Tooltip("If true, camera will smoothly interpolate towards the target")]
	    public bool smoothFollow = true;
	    
	    [BoxGroup("Position")]
	    [Tooltip("Smooth follow speed")]
	    [Range(0,10)]
	    public float followSpeed = 10f;
	    
	    
	    [BoxGroup("Rotation")]
	    public ThirdPersonCamera.RotationMode rotationMode;
	
	    [BoxGroup("Rotation")]
	    [Tooltip("If assigned, will use this Transform's rotation as the rotation space instead of the world space. Useful with spherical planets.")]
	    public Transform rotationSpace; 
	
	    [BoxGroup("Rotation")] 
	    //[MinMaxSlider(-180, 180, true)]
	    public Vector2 rotationRange = new Vector2(0, 90);
	    
	    [BoxGroup("Rotation")]
	    //[PropertyRange(0,10)]
	    public float rotationSpeed = 5f;
	    
	    [BoxGroup("Rotation")]
	    //[PropertyRange(0, 5)]
	    public float rotationDuration = 1;

	    
	    [BoxGroup("Zoom")] 
        public float distance = 10.0f, minDistance = 4, maxDistance = 10;
        
        [BoxGroup("Zoom")] 
        [Range(0,10)]
        public float zoomSpeed = 5f;

        [BoxGroup("Blocking")]
        public LayerMask blockingLayers;
		
        [BoxGroup("Blocking")]
        public float blockingRadius = 1f, blockingSmoothTime = 0.1f, blockingOriginOffset;
		
        [BoxGroup("Blocking")]
        [Range(0, 1f)]
        public float blockedOffset = 0.5f;
    }
}