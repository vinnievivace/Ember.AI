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

        public enum SizeMode { Scale, Tile }
        
        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        [BoxGroup("Settings"), SerializeField, OnValueChanged(nameof(DoGeneratePlane))]
        private Material material;
        
        [BoxGroup("Settings"), SerializeField, OnValueChanged(nameof(DoGeneratePlane))]
        private SizeMode gridSizeMode = SizeMode.Tile;
        
        
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

        public override void EditModeInitialize()
        {
            base.EditModeInitialize();
            
            if(material != null) GeneratePlane(gridSize, material);

            
        }

        #endregion

        #region MonoBehaviours .........................................................................................

        #endregion

        #region General ................................................................................................

        private void GeneratePlane(int size, Material mat)
        {
            // Get or add render components
            var meshFilter   = this.GetOrAddComponent<MeshFilter>();
            var meshRenderer = this.GetOrAddComponent<MeshRenderer>();

            // Remove any leftover MeshCollider
            var oldMeshCol = GetComponent<MeshCollider>();
            if (oldMeshCol != null) DestroyImmediate(oldMeshCol);

            // Ensure a BoxCollider (we’ll size it below)
            var boxCollider = this.GetOrAddComponent<BoxCollider>();

            // Assign material once
            if (mat != null) meshRenderer.material = mat;

            if (gridSizeMode == SizeMode.Scale)
            {
                // —— SCALE MODE ——  
                // Build a simple 1×1 quad centered at the origin
                var quad = new Mesh { name = "GroundPlane_Scaled" };
                quad.vertices = new Vector3[]
                {
                    new Vector3(-0.5f, 0, -0.5f),
                    new Vector3( 0.5f, 0, -0.5f),
                    new Vector3(-0.5f, 0,  0.5f),
                    new Vector3( 0.5f, 0,  0.5f),
                };
                quad.uv = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                };
                quad.triangles = new int[] { 0, 2, 1,  1, 2, 3 };
                quad.RecalculateNormals();

                meshFilter.sharedMesh = quad;

                // Stretch the plane in X/Z
                transform.localScale = new Vector3(size, 1, size);

                // A unit-sized BoxCollider, pivoted at origin
                boxCollider.size   = new Vector3(1f, 0.1f, 1f);
                boxCollider.center = new Vector3(0f, -0.05f, 0f);
            }
            else
            {
                // —— TILE MODE ——  
                // Reset any scaling
                transform.localScale = Vector3.one;

                // Procedural subdivided mesh with 1-unit tiles
                var mesh = new Mesh { name = "GroundPlane_Tiled" };
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

                int vertsPerLine = size + 1;
                int totalVerts   = vertsPerLine * vertsPerLine;
                var vertices = new Vector3[totalVerts];
                var uvs       = new Vector2[totalVerts];
                var tris      = new int[size * size * 6];

                float half = size * 0.5f;
                int vi = 0;
                for (int z = 0; z <= size; z++)
                {
                    for (int x = 0; x <= size; x++)
                    {
                        // center pivot at world origin
                        vertices[vi] = new Vector3(x - half, 0, z - half);
                        // tile UVs one per unit
                        uvs[vi] = new Vector2(x, z);
                        vi++;
                    }
                }

                int ti = 0;
                for (int z = 0; z < size; z++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        int topLeft    = z * vertsPerLine + x;
                        int bottomLeft = (z + 1) * vertsPerLine + x;

                        tris[ti++] = topLeft;
                        tris[ti++] = bottomLeft;
                        tris[ti++] = topLeft + 1;

                        tris[ti++] = topLeft + 1;
                        tris[ti++] = bottomLeft;
                        tris[ti++] = bottomLeft + 1;
                    }
                }

                mesh.vertices  = vertices;
                mesh.uv        = uvs;
                mesh.triangles = tris;
                mesh.RecalculateNormals();

                meshFilter.sharedMesh = mesh;

                // BoxCollider spanning the plane
                boxCollider.size   = new Vector3(size, 0.1f, size);
                boxCollider.center = new Vector3(0f, -0.05f, 0f);
            }

            // Finally, apply your ground‐layer
            if (EnvironmentManager.Instance != null)
                gameObject.layer = EnvironmentManager.Instance.GetGroundLayerIndex();
        }
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

#endregion
    }
}