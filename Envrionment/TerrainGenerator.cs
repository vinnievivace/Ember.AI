using UnityEngine;
using EmberAI.Attributes;

namespace EmberAI.Envrionment
{
    /// <summary>
    /// Generates realistic terrain for testing foot IK and character movement.
    /// Creates varied landscapes with hills, valleys, slopes, and elevation changes.
    /// </summary>
    public class TerrainGenerator : EmberBehaviour
    {
        [Header("Terrain Settings")]
        [SerializeField, Tooltip("Width of the terrain in world units")]
        private int terrainWidth = 256;
        
        [SerializeField, Tooltip("Length of the terrain in world units")]
        private int terrainLength = 256;
        
        [SerializeField, Tooltip("Maximum height of the terrain")]
        private float maxHeight = 20f;
        
        [SerializeField, Tooltip("Scale of the terrain (higher = more detailed)")]
        private float terrainScale = 50f;

        [Header("Noise Settings")]
        [SerializeField, Tooltip("Base noise scale for terrain generation")]
        private float baseNoiseScale = 0.01f;
        
        [SerializeField, Tooltip("Detail noise scale for fine terrain features")]
        private float detailNoiseScale = 0.05f;
        
        [SerializeField, Tooltip("Weight of detail noise (0-1)")]
        [Range(0f, 1f)]
        private float detailWeight = 0.3f;
        
        [SerializeField, Tooltip("Number of noise octaves for fractal terrain")]
        [Range(1, 8)]
        private int octaves = 4;
        
        [SerializeField, Tooltip("Persistence between octaves (0-1)")]
        [Range(0f, 1f)]
        private float persistence = 0.5f;
        
        [SerializeField, Tooltip("Lacunarity (frequency multiplier between octaves)")]
        [Range(1f, 4f)]
        private float lacunarity = 2f;

        [Header("Terrain Features")]
        [SerializeField, Tooltip("Add a central plateau for flat ground testing")]
        private bool addCentralPlateau = true;
        
        [SerializeField, Tooltip("Radius of the central plateau")]
        private float plateauRadius = 30f;
        
        [SerializeField, Tooltip("Height of the central plateau")]
        private float plateauHeight = 2f;
        
        [SerializeField, Tooltip("Add gradual slopes around the plateau")]
        private bool addSlopes = true;
        
        [SerializeField, Tooltip("Add some steep cliffs for extreme testing")]
        private bool addCliffs = true;
        
        [SerializeField, Tooltip("Number of cliffs to generate")]
        [Range(1, 5)]
        private int cliffCount = 3;

        [Header("Material Settings")]
        [SerializeField, Tooltip("Terrain material to apply")]
        private Material terrainMaterial;

        [Header("Physics Settings")]
        [SerializeField, Tooltip("Layer mask for the terrain (should match EnvironmentManager ground layer)")]
        private LayerMask groundLayer = 1;
        
        [SerializeField, Tooltip("Add terrain collider for physics interactions")]
        private bool addTerrainCollider = true;

        [Header("Debug")]
        [SerializeField, Tooltip("Show terrain generation gizmos")]
        private bool showGizmos = true;

        private Terrain _terrain;
        private TerrainData _terrainData;
        private Vector3 _terrainPosition;

        #region Unity Methods

        public override void EditModeInitialize()
        {
            base.EditModeInitialize();
            description = "Generates realistic terrain for testing foot IK and character movement.";
        }

        protected override void OnStart()
        {
            base.OnStart();
            GenerateTerrain();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates a new terrain with the current settings.
        /// </summary>
        [ContextMenu("Generate Terrain")]
        public void GenerateTerrain()
        {
            CreateTerrainGameObject();
            GenerateHeightMap();
            ApplyTerrainFeatures();
            SetupTerrainMaterial();
            SetupTerrainPhysics();
            
            Debug.Log($"Terrain generated successfully! Size: {terrainWidth}x{terrainLength}, Max Height: {maxHeight}");
        }

        /// <summary>
        /// Clears the current terrain and removes it from the scene.
        /// </summary>
        [ContextMenu("Clear Terrain")]
        public void ClearTerrain()
        {
            if (_terrain != null)
            {
                DestroyImmediate(_terrain.gameObject);
                _terrain = null;
                _terrainData = null;
            }
        }

        #endregion

        #region Private Methods

        private void CreateTerrainGameObject()
        {
            // Remove existing terrain
            ClearTerrain();

            // Create new terrain
            GameObject terrainObject = new GameObject("Generated Terrain");
            terrainObject.transform.SetParent(transform);
            
            _terrain = terrainObject.AddComponent<Terrain>();
            _terrainData = new TerrainData();
            _terrain.terrainData = _terrainData;

            // Set terrain size
            _terrainData.size = new Vector3(terrainWidth, maxHeight, terrainLength);
            _terrainPosition = terrainObject.transform.position;
            
            // Position terrain at origin
            terrainObject.transform.position = Vector3.zero;
        }

        private void GenerateHeightMap()
        {
            int heightmapResolution = _terrainData.heightmapResolution;
            float[,] heights = new float[heightmapResolution, heightmapResolution];

            for (int x = 0; x < heightmapResolution; x++)
            {
                for (int y = 0; y < heightmapResolution; y++)
                {
                    float xCoord = (float)x / heightmapResolution * terrainScale;
                    float yCoord = (float)y / heightmapResolution * terrainScale;

                    // Generate base noise
                    float baseHeight = GenerateFractalNoise(xCoord, yCoord, baseNoiseScale, octaves, persistence, lacunarity);
                    
                    // Generate detail noise
                    float detailHeight = GenerateFractalNoise(xCoord, yCoord, detailNoiseScale, octaves, persistence, lacunarity);
                    
                    // Combine base and detail noise
                    float combinedHeight = baseHeight + (detailHeight * detailWeight);
                    
                    // Normalize to 0-1 range
                    heights[x, y] = Mathf.Clamp01(combinedHeight);
                }
            }

            _terrainData.SetHeights(0, 0, heights);
        }

        private void ApplyTerrainFeatures()
        {
            if (!addCentralPlateau && !addSlopes && !addCliffs) return;

            int heightmapResolution = _terrainData.heightmapResolution;
            float[,] heights = _terrainData.GetHeights(0, 0, heightmapResolution, heightmapResolution);

            Vector3 terrainSize = _terrainData.size;
            Vector3 center = new Vector3(terrainSize.x * 0.5f, 0, terrainSize.z * 0.5f);

            for (int x = 0; x < heightmapResolution; x++)
            {
                for (int y = 0; y < heightmapResolution; y++)
                {
                    // Convert heightmap coordinates to world coordinates
                    float worldX = (float)x / heightmapResolution * terrainSize.x;
                    float worldZ = (float)y / heightmapResolution * terrainSize.z;
                    Vector3 worldPos = new Vector3(worldX, 0, worldZ);

                    float distanceFromCenter = Vector3.Distance(worldPos, center);

                    // Apply central plateau
                    if (addCentralPlateau && distanceFromCenter < plateauRadius)
                    {
                        float plateauInfluence = 1f - (distanceFromCenter / plateauRadius);
                        plateauInfluence = Mathf.SmoothStep(0f, 1f, plateauInfluence);
                        
                        float plateauHeightNormalized = plateauHeight / terrainSize.y;
                        heights[x, y] = Mathf.Lerp(heights[x, y], plateauHeightNormalized, plateauInfluence * 0.8f);
                    }

                    // Apply gradual slopes
                    if (addSlopes && distanceFromCenter > plateauRadius && distanceFromCenter < plateauRadius * 2f)
                    {
                        float slopeInfluence = (distanceFromCenter - plateauRadius) / plateauRadius;
                        slopeInfluence = Mathf.SmoothStep(0f, 1f, slopeInfluence);
                        
                        // Create gentle slopes that gradually increase in height
                        float slopeHeight = Mathf.Lerp(plateauHeight / terrainSize.y, 0.3f, slopeInfluence);
                        heights[x, y] = Mathf.Lerp(heights[x, y], slopeHeight, 0.6f);
                    }

                    // Apply cliffs
                    if (addCliffs)
                    {
                        for (int i = 0; i < cliffCount; i++)
                        {
                            float angle = (360f / cliffCount) * i * Mathf.Deg2Rad;
                            Vector3 cliffCenter = center + new Vector3(
                                Mathf.Cos(angle) * plateauRadius * 1.5f,
                                0,
                                Mathf.Sin(angle) * plateauRadius * 1.5f
                            );

                            float distanceFromCliff = Vector3.Distance(worldPos, cliffCenter);
                            if (distanceFromCliff < 15f)
                            {
                                float cliffInfluence = 1f - (distanceFromCliff / 15f);
                                cliffInfluence = Mathf.SmoothStep(0f, 1f, cliffInfluence);
                                
                                // Create steep cliffs
                                float cliffHeight = Mathf.Lerp(0.1f, 0.8f, cliffInfluence);
                                heights[x, y] = Mathf.Lerp(heights[x, y], cliffHeight, cliffInfluence * 0.9f);
                            }
                        }
                    }
                }
            }

            _terrainData.SetHeights(0, 0, heights);
        }

        private void SetupTerrainMaterial()
        {
            if (terrainMaterial != null)
            {
                _terrain.materialTemplate = terrainMaterial;
            }
            else
            {
                // Create a basic terrain material if none is assigned
                Material basicMaterial = new Material(Shader.Find("Nature/Terrain/Standard"));
                _terrain.materialTemplate = basicMaterial;
            }
        }

        private void SetupTerrainPhysics()
        {
            // Set the terrain layer for foot IK raycasts
            _terrain.gameObject.layer = GetLayerFromMask(groundLayer);
            
            // Add terrain collider for physics interactions
            if (addTerrainCollider)
            {
                TerrainCollider terrainCollider = _terrain.gameObject.GetComponent<TerrainCollider>();
                if (terrainCollider == null)
                {
                    terrainCollider = _terrain.gameObject.AddComponent<TerrainCollider>();
                }
                terrainCollider.terrainData = _terrainData;
            }
            
            Debug.Log($"Terrain physics setup complete. Layer: {LayerMask.LayerToName(_terrain.gameObject.layer)}");
        }

        private int GetLayerFromMask(LayerMask layerMask)
        {
            int layer = 0;
            int mask = layerMask.value;
            
            while (mask > 0)
            {
                if ((mask & 1) != 0)
                {
                    return layer;
                }
                mask >>= 1;
                layer++;
            }
            
            return 0; // Default to layer 0 if no layer found
        }

        private float GenerateFractalNoise(float x, float y, float scale, int octaves, float persistence, float lacunarity)
        {
            float amplitude = 1f;
            float frequency = 1f;
            float noiseHeight = 0f;
            float maxValue = 0f;

            for (int i = 0; i < octaves; i++)
            {
                float sampleX = x / scale * frequency;
                float sampleY = y / scale * frequency;

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;
                noiseHeight += perlinValue * amplitude;

                maxValue += amplitude;
                amplitude *= persistence;
                frequency *= lacunarity;
            }

            return noiseHeight / maxValue;
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (!showGizmos || _terrain == null) return;

            Vector3 terrainSize = _terrainData != null ? _terrainData.size : new Vector3(terrainWidth, maxHeight, terrainLength);
            Vector3 center = transform.position + terrainSize * 0.5f;

            // Draw terrain bounds
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(center, terrainSize);

            // Draw plateau area
            if (addCentralPlateau)
            {
                Gizmos.color = Color.yellow;
                Vector3 plateauCenter = transform.position + new Vector3(terrainSize.x * 0.5f, plateauHeight, terrainSize.z * 0.5f);
                Gizmos.DrawWireSphere(plateauCenter, plateauRadius);
            }

            // Draw cliff positions
            if (addCliffs && _terrainData != null)
            {
                Gizmos.color = Color.red;
                Vector3 centerPos = transform.position + new Vector3(terrainSize.x * 0.5f, 0, terrainSize.z * 0.5f);
                
                for (int i = 0; i < cliffCount; i++)
                {
                    float angle = (360f / cliffCount) * i * Mathf.Deg2Rad;
                    Vector3 cliffCenter = centerPos + new Vector3(
                        Mathf.Cos(angle) * plateauRadius * 1.5f,
                        0,
                        Mathf.Sin(angle) * plateauRadius * 1.5f
                    );
                    Gizmos.DrawWireSphere(cliffCenter, 15f);
                }
            }
        }

        #endregion
    }
} 