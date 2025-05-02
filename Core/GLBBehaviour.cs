using System.Threading.Tasks;
using EmberAI.Attributes;
using EmberAI.Avatars;
using EmberAI.Core.Util;
using GLTFast;
using UnityEngine;
using AvatarBuilder = EmberAI.Avatars.AvatarBuilder;

namespace EmberAI.Core
{
    public class GLBBehaviour : EmberBehaviour
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

        [BoxGroup("Settings"), OnValueChanged(nameof(OnChangeType))] 
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
            Avatar avatar = AvatarBuilder.Build(transform, avatarConfig, FileUtil.Combine(avatarOutputFolder, avatarOutputName + "_Avatar.asset"));

            SetHumanoidAvatar(avatar);
        }
        
        /// <summary>
    /// Places any avatar (GLB prefab, humanoid or not) so its lowest visible vertex
    /// lands exactly at groundPosition.y, and moves XZ to groundPosition as well.
    /// Logs mesh counts, computed minY, offset, old/new Y positions.
    /// </summary>
    public static void PlaceAvatarOnGround(GameObject avatar, Vector3 groundPosition)
    {
        float minY = float.MaxValue;

        // 1) MeshFilters
        var meshFilters = avatar.GetComponentsInChildren<MeshFilter>();
        foreach (var mf in meshFilters)
        {
            Mesh mesh = mf.sharedMesh;
            if (mesh == null) continue;

            Vector3[] verts = mesh.vertices;
            var t = mf.transform;
            for (int i = 0; i < verts.Length; i++)
            {
                float worldY = t.TransformPoint(verts[i]).y;
                if (worldY < minY) minY = worldY;
            }

            Debug.Log($"[PlaceAvatar] MeshFilter '{mf.name}' → {verts.Length} verts scanned.");
        }

        // 2) SkinnedMeshRenderers (bake into a temp mesh)
        var skinned = avatar.GetComponentsInChildren<SkinnedMeshRenderer>();
        var bakeMesh = new Mesh();
        foreach (var smr in skinned)
        {
            smr.BakeMesh(bakeMesh);
            Vector3[] verts = bakeMesh.vertices;
            var t = smr.transform;
            for (int i = 0; i < verts.Length; i++)
            {
                float worldY = t.TransformPoint(verts[i]).y;
                if (worldY < minY) minY = worldY;
            }

            Debug.Log($"[PlaceAvatar] SkinnedMeshRenderer '{smr.name}' → {verts.Length} baked verts scanned.");
        }

        if (minY == float.MaxValue)
        {
            Debug.LogWarning("[PlaceAvatar] No mesh data found on avatar!");
            return;
        }

        // 3) Compute and apply offset
        float offsetY = groundPosition.y - minY;
        var oldPos = avatar.transform.position;
        var newPos = new Vector3(
            groundPosition.x,
            oldPos.y + offsetY,
            groundPosition.z
        );
        avatar.transform.position = newPos;

        Debug.Log(
            $"[PlaceAvatar] worldMinY: {minY:F4}, " +
            $"groundY: {groundPosition.y:F4}, " +
            $"offsetY: {offsetY:F4}, " +
            $"oldY: {oldPos.y:F4}, newY: {newPos.y:F4}"
        );
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
            
            // some avatars may have animations baked in, so we need to remove the legacy Animation component
            Animation animationComponent = gameObject.GetComponentInChildren<Animation>();
            
            
            animationComponent.transform.localRotation = avatarConfig.rotationOffset;
            
            animationComponent.RemoveComponent<Animation>();

            CharacterController controller = gameObject.GetComponent<CharacterController>();
            controller.center = new Vector3(0, avatarConfig.yOffset, 0);

        }
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

#endregion
    }
}