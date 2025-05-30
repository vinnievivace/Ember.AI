using System.Threading.Tasks;
using EmberAI.Attributes;
using EmberAI.Avatars;
using GLTFast;
using GLTFast.Logging;
using UnityEngine;
using UnityEditor;
using AvatarBuilder = EmberAI.Avatars.AvatarBuilder;
using FileUtil = EmberAI.Core.Util.FileUtil;

namespace EmberAI.Core
{
    public class GLBBehaviour : EmberBehaviour
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        public event System.Action<string> OnLoadComplete;
        public event System.Action<string> OnLoadError;

        #endregion
        
        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        public enum GLBType { Standard, Humanoid }
        
        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        private GameObject _loadedModel;
        private Task _loadTask;
       
        
        [BoxGroup("Settings")]
        public string path;

        [BoxGroup("Settings"), OnValueChanged(nameof(OnChangeType))] 
        public GLBType type;

        [BoxGroup("Settings")] 
        public bool saveToCache;
        
        [BoxGroup("Components"), Tooltip("If Humanoid, full 3rd person controller system is enabled"), SerializeField]
        private CharacterControllerSystem ControllerSystem;
        
        [BoxGroup("Avatar")]
        public AvatarConfig avatarConfig;
        
        [BoxGroup("Debug")] 
        public bool rebuildAvatar, debugBoneRotations;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        #endregion

        #region Inspector ..............................................................................................

        private void OnChangeType()
        {
            if (type == GLBType.Humanoid)
            {
                SetupHumanoidController();
            }
            else
            {
                RemoveHumanoidController();
            }
        }
        
        #endregion

        #region Initialization .........................................................................................

        public override void InitializeDependencies()
        {
            base.InitializeDependencies();

            description = "GLB Load / Avatar creation";

            OnChangeType();
            
        }

        #endregion

        #region MonoBehaviours .........................................................................................

        protected override void OnAwake()
        {
            base.OnAwake();

            if(path.IsEmptyString()) return;

            _loadTask = LoadGLBAsync(path);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            
            if(ControllerSystem) ControllerSystem.active = !debugBoneRotations;

            // very rough, just a way to debug our bone rotations at runtime.
            if (debugBoneRotations && _loadedModel != null)
            {
                AvatarBuilder.ApplyBoneRotations(transform, avatarConfig);
                AvatarBuilder.ApplyShoulderOffset(transform, avatarConfig);
            }
        }

        #endregion

        #region General ................................................................................................

        /// <summary>
        /// Destroys the currently loaded GLB model
        /// </summary>
        public void Unload()
        {
            if (_loadedModel != null)
            {
                Destroy(_loadedModel);
                _loadedModel = null;
            }
        }
        
        public void LoadGLB(string path)
        {
            this.path = path;
            
            Unload();
            
            if(path.IsEmptyString()) return;

            if (_loadTask != null && !_loadTask.IsCompleted)
            {
                Debug.LogWarning("Cannot load " + path + " while another GLB is loading.");
                
                return;
            }
            
            _loadTask = LoadGLBAsync(path);
        }
        
        private async Task LoadGLBAsync(string path)
        {
            GltfImport gltf = new GltfImport(null, null, null, new ConsoleLogger());
            ImportSettings settings = new ImportSettings { AnimationMethod = AnimationMethod.None};
            bool success;

            if (path.StartsWith("http"))
                success = await gltf.Load(new System.Uri(path), settings);
            else
                success = await gltf.Load(path, settings);

            if (success)
            {
                _loadedModel = new GameObject("GLB_Model");
                _loadedModel.SetParent(this);
                
                await gltf.InstantiateMainSceneAsync(_loadedModel.transform);

                if (type == GLBType.Humanoid)
                {
                    Avatar avatar = LoadAvatar(avatarConfig);

                    if (avatar == null || rebuildAvatar)
                    {
                        CreateAvatar();
                    }
                    else
                    {
                        SetHumanoidAvatar(avatar);
                    }
                }
                
                // bit of a hack and can probably be removed.
                AvatarBuilder.ApplyBoneRotations(transform, avatarConfig);

                DispatchEvent(OnLoadComplete, path);
                
            }
            else
            {
                DispatchEvent(OnLoadError, "Failed to load GLB: " + path, LogLevel.Error);
            }
        }
        
        private Avatar LoadAvatar(AvatarConfig config)
        {
            #if UNITY_EDITOR
            string avatarPath = FileUtil.Combine("Assets/", AvatarBuilder.OutputFolderName, config.name + ".asset");
            
            return AssetDatabase.LoadAssetAtPath<Avatar>(avatarPath);
            #else
            return null;
            #endif
        }

        private void CreateAvatar()
        {
            string avatarOutputFolder = FileUtil.CombineWithDataPath(AvatarBuilder.OutputFolderName);
            Avatar avatar = AvatarBuilder.Build(transform, avatarConfig, FileUtil.Combine(avatarOutputFolder, avatarConfig.name + ".asset"));

            SetHumanoidAvatar(avatar);
        }
     
        #endregion
        
        #region Humanoid ...............................................................................................

        private void SetupHumanoidController()
        {
            if(type != GLBType.Humanoid) throw new System.Exception("HumanoidController can only be set on humanoid GLB");
            
            KeyboardMouseInput input = gameObject.GetOrAddComponent<KeyboardMouseInput>();
            
            ControllerSystem = gameObject.GetOrAddComponent<CharacterControllerSystem>();
            ControllerSystem.characterInput = input;
            
        }

        private void RemoveHumanoidController()
        {
            if(ControllerSystem == null) return;
            
            ControllerSystem.DestroyDependencies();
            
            this.RemoveComponent<CharacterControllerSystem>();
            
        }

        // At runtime the avatar is generated and can then be applied
        public void SetHumanoidAvatar(Avatar avatar, bool resetYPosition = true)
        {
            if(type != GLBType.Humanoid) throw new System.Exception("Avatar can only be set on humanoid GLB");
            
            gameObject.GetComponent<CharacterControllerSystem>().ApplyAvatar(avatar, avatarConfig);
            
            // some avatars may have animations baked in, so we need to remove the legacy Animation component that gets attached.
            Animation animationComponent = gameObject.GetComponentInChildren<Animation>();
            animationComponent.RemoveComponent<Animation>();
            
            // maybe a bit fragile, apply rotation to the first child...
            transform.GetChild(0).localEulerAngles = avatarConfig.rotationOffset;
            
            // all very hacky. need to ensure new avatar not spawned below the ground.
            if (resetYPosition)
            {
                CharacterControllerSystem controllerSystem  = gameObject.GetComponent<CharacterControllerSystem>();
                CharacterController controller = gameObject.GetComponent<CharacterController>();
                controller.center = new Vector3(0, avatarConfig.yOffset, 0);
                controllerSystem.active = false;
            
                transform.position = new Vector3(transform.position.x, transform.position.y + 5, transform.position.z);
            
                CallbackManager.AddOneOff(this, 0.5f, () => { controllerSystem.active = true; });
            }
        }
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

#endregion
    }
}