
using System.Threading.Tasks;
using EmberAI.Core;
using EmberAI.Attributes;
using EmberAI.Avatars;
using EmberAI.Core.Util;
using GLTFast;
using UnityEngine;
using AvatarBuilder = EmberAI.Avatars.AvatarBuilder;

namespace EmberAI
{
    public class GLBBlock : BaseBlock
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        public enum GLBType { Standard, Humanoid }
        
        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        private static readonly int Special = Animator.StringToHash("Special");
        
        private GameObject _loadedModel;

        [BoxGroup("Settings")]
        public string path;

        [BoxGroup("Settings")] 
        public GLBType type;

        [BoxGroup("Settings")] 
        public bool saveToCache;
        
        [BoxGroup("Avatar")]
        public AvatarConfig avatarConfig;

        [BoxGroup("Avatar")] 
        public string avatarOutputFolder, avatarOutputName;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        #endregion

        #region Inspector ..............................................................................................

        #endregion

        #region Initialization .........................................................................................

        public override void EditorRebuild()
        {
            base.EditorRebuild();

            description = "GLB Load / Avatar creation";
            
        }

        #endregion

        #region MonoBehaviours .........................................................................................

        protected override void OnAwake()
        {
            base.OnAwake();

            LoadGLBAsync(path);
        }

        #endregion

        #region General ................................................................................................

        private async Task LoadGLBAsync(string path)
        {
            GltfImport gltf = new GltfImport();
            bool success;

            if (path.StartsWith("http"))
                success = await gltf.Load(new System.Uri(path));
            else
                success = await gltf.Load(path);

            if (success)
            {
                _loadedModel = new GameObject("GLB_Model");
                _loadedModel.SetParent(this);
                
                await gltf.InstantiateMainSceneAsync(_loadedModel.transform);
                // on success event
                
                if(type == GLBType.Humanoid) CreateAvatar();
            }
            else
            {
                Debug.LogError("Failed to load GLB: " + path);
                // on fail event
            }
        }

        private void CreateAvatar()
        {
            AvatarBuilder.Build(transform, avatarConfig, FileUtil.Combine(avatarOutputFolder, avatarOutputName + "_Avatar.asset"));

            transform.eulerAngles = new Vector3(-90, 180, 0);
            transform.position = new Vector3(0, 0.5f, 0);
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            
            GetComponent<Animator>().SetTrigger(Special);
        }
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

#endregion
    }
}