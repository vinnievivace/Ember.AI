using System.Threading.Tasks;
using EmberAI.Attributes;
using EmberAI.Avatars;
using GLTFast;
using GLTFast.Logging;
using GLTFast.Materials;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
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

        private static readonly int Special = Animator.StringToHash("Special");
        
        private GameObject _loadedModel;
        private Task _loadTask;
        
        [BoxGroup("Settings")]
        public string path;

        [BoxGroup("Settings"), OnValueChanged(nameof(OnChangeType))] 
        public GLBType type;

        [BoxGroup("Settings")] 
        public bool saveToCache;
        
        [BoxGroup("Avatar")]
        public AvatarConfig avatarConfig;
        
        [BoxGroup("Debug")] 
        public bool rebuildAvatar;
        
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
            
            CharacterControllerSystem controller = gameObject.GetOrAddComponent<CharacterControllerSystem>();
            KeyboardMouseInput input = gameObject.GetOrAddComponent<KeyboardMouseInput>();
            
            controller.characterInput = input;
            
        }

        private void RemoveHumanoidController()
        {
            CharacterControllerSystem controller = gameObject.GetComponent<CharacterControllerSystem>();
            
            if(controller == null) return;
            
            controller.DestroyDependencies();
            this.RemoveComponent<CharacterControllerSystem>();
            
        }

        // At runtime the avatar is generated and can then be applied
        private void SetHumanoidAvatar(Avatar avatar)
        {
            if(type != GLBType.Humanoid) throw new System.Exception("Avatar can only be set on humanoid GLB");
            
            gameObject.GetComponent<CharacterControllerSystem>().ApplyAvatar(avatar);
            
            // some avatars may have animations baked in, so we need to remove the legacy Animation component that gets attached.
            Animation animationComponent = gameObject.GetComponentInChildren<Animation>();
            animationComponent.RemoveComponent<Animation>();
            
            // maybe a bit fragile, apply rotation to the first child...
            transform.GetChild(0).localEulerAngles = avatarConfig.rotationOffset;
            
            // all very hacky. need to ensure new avatar not spawned below the ground.
            CharacterControllerSystem controllerSystem  = gameObject.GetComponent<CharacterControllerSystem>();
            CharacterController controller = gameObject.GetComponent<CharacterController>();
            controller.center = new Vector3(0, avatarConfig.yOffset, 0);
            controllerSystem.active = false;
            
            transform.position = new Vector3(transform.position.x, transform.position.y + 5, transform.position.z);
            
            Debug.Log(transform.name + " : " + transform.position);
            
            CallbackManager.AddOneOff(this, 0.5f, () => { controllerSystem.active = true; });
        }
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

#endregion
    }
}