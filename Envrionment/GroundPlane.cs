using EmberAI.Attributes;
using EmberAI.Core;
using UnityEngine;

namespace EmberAI.Envrionment
{
    public class GroundPlane : EmberBehaviour
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        [BoxGroup("Settings"), SerializeField, OnValueChanged(nameof(DoGeneratePlane))]
        private Material material;
        
        [BoxGroup("Settings"), SerializeField, OnValueChanged(nameof(DoGeneratePlane))]
        private int gridSize = 10;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        #endregion

        #region Inspector ..............................................................................................

        private void DoGeneratePlane()
        {
            if(material != null) GeneratePlane(gridSize, material);;
        }
        
        #endregion

        #region Initialization .........................................................................................

        public override void InitializeDependencies()
        {
            base.InitializeDependencies();
            
            if(material != null) GeneratePlane(gridSize, material);

            
        }

        #endregion

        #region MonoBehaviours .........................................................................................

        #endregion

        #region General ................................................................................................

        private void GeneratePlane(int size, Material mat)
        {
            MeshFilter meshFilter = this.GetOrAddComponent<MeshFilter>();
            MeshRenderer meshRenderer = this.GetOrAddComponent<MeshRenderer>();

            // Clean up old collider
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
                DestroyImmediate(meshCollider);

            // Ensure BoxCollider exists and is accurate
            BoxCollider boxCollider = this.GetOrAddComponent<BoxCollider>();
            boxCollider.size = new Vector3(size, 0.1f, size); // flat, square
            boxCollider.center = new Vector3(size * 0.5f, -0.05f, size * 0.5f); // flush with y=0

            // Clean up old mesh
            if (meshFilter.sharedMesh != null)
                DestroyImmediate(meshFilter.sharedMesh);

            // Generate flat plane mesh
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; 

            Vector3[] vertices = new Vector3[(size + 1) * (size + 1)];
            Vector2[] uvs = new Vector2[vertices.Length];
            int[] triangles = new int[size * size * 6];

            int vertIndex = 0;
            for (int z = 0; z <= size; z++)
            {
                for (int x = 0; x <= size; x++)
                {
                    float halfSize = size * 0.5f;
                    vertices[vertIndex] = new Vector3(x - halfSize, 0, z - halfSize);
          
                    uvs[vertIndex] = new Vector2(x, z);                      
                    vertIndex++;
                }
            }

            int triIndex = 0;
            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    int topLeft = z * (size + 1) + x;
                    int bottomLeft = (z + 1) * (size + 1) + x;

                    triangles[triIndex++] = topLeft;
                    triangles[triIndex++] = bottomLeft;
                    triangles[triIndex++] = topLeft + 1;

                    triangles[triIndex++] = topLeft + 1;
                    triangles[triIndex++] = bottomLeft;
                    triangles[triIndex++] = bottomLeft + 1;
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            meshFilter.sharedMesh = mesh;

            if (mat != null)
                meshRenderer.material = mat;

            if (EnvironmentManager.Instance != null)
                gameObject.layer = EnvironmentManager.Instance.GetGroundLayerIndex();
        }


        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

#endregion
    }
}