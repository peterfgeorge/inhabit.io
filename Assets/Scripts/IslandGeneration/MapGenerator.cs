using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using System;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {NoiseMap, ColourMap, Mesh, FalloffMap}
    public DrawMode drawMode;
    public List<Biome> biomes;
    public List<IslandBiome> islandBiomes;
    public const int mapChunkSize = 239;
    public bool useFlatShading;
    [Range(0,6)]
    public int levelOfDetail;
    public float noiseScale;

    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;
    public bool useFalloff;
    public bool removeSmallerIslands;
    public bool addBiomes;
    public bool blendBiomes;

    [Range(0,8)]
    public int numberOfBiomes;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
 
    public bool autoUpdate;

    public TerrainType[] regions;
    float[,] falloffMap;
    private float[,] noiseMap;
    private float[,] biomeSpecificHeights = new float[mapChunkSize, mapChunkSize];

    void Awake() {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }
    public void GenerateMap() {
        CleanUpPrevMapGeneration();

        noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

        if (useFalloff) {
            for (int y = 0; y < mapChunkSize; y++) {
                for (int x = 0; x < mapChunkSize; x++) {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                }
            }
        }

        if (removeSmallerIslands) {
            noiseMap = IslandRemover.RemoveSmallerIslands(noiseMap, 0.35f, 10); // Remove smaller islands before coloring
        }

        List<Vector2Int> mainIsland = IslandRemover.GetMainIslandCoordinates(noiseMap, 0.35f);
        islandBiomes = new List<IslandBiome>(); // Initialize the IslandBiome list

        List<HashSet<Vector2Int>> biomeAreas  = new List<HashSet<Vector2Int>>();

        if (addBiomes && biomes.Count > 0) // Check if biomes should be added
        {
            biomeAreas = BiomeGenerator.DivideMainIslandIntoBiomes(mainIsland, numberOfBiomes);

            // Create IslandBiomes based on the biome areas
            for (int i = 0; i < biomeAreas.Count; i++)
            {
                Biome randomBiome = biomes[UnityEngine.Random.Range(0, biomes.Count)];
                IslandBiome islandBiome = gameObject.AddComponent<IslandBiome>();
                islandBiome.SetBiome(randomBiome);

                // Assign tiles to the biome
                foreach (Vector2Int tile in biomeAreas[i])
                {
                    islandBiome.tiles.Add(tile);
                    // Determine if this is a border tile (e.g., adjacent to non-biome tiles)
                    if (IsBorderTile(tile, biomeAreas[i], noiseMap))
                    {
                        islandBiome.borderTiles.Add(tile);
                    }
                }
                islandBiomes.Add(islandBiome); // Add the generated IslandBiome to the list
            }
        }

        // Update the colourMap based on the modified noiseMap
        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];

        int[] biomeAssignments = new int[biomeAreas.Count];

        for(int i=0; i < biomeAreas.Count; i++) {
            biomeAssignments[i] = UnityEngine.Random.Range(0, biomes.Count);
        }

        // Blend biomes if the option is enabled
        if (blendBiomes) {
            BlendBiomes blending = new BlendBiomes(islandBiomes, noiseMap);
            float[,] biomeBlendedHeights = new float[mapChunkSize, mapChunkSize];
            Array.Copy(biomeSpecificHeights, biomeBlendedHeights, biomeSpecificHeights.Length);
            blending.BlendBiomeHeights(biomeBlendedHeights);
            // Assign colors to the biome-specific heights
            for (int y = 0; y < mapChunkSize; y++) {
                for (int x = 0; x < mapChunkSize; x++) {
                    float currentHeight = noiseMap[x, y];
                    Vector2Int currentTile = new Vector2Int(x, y);

                    // Check which biome the current tile belongs to and assign the corresponding color
                    bool isBiomeTile = false;

                    foreach (IslandBiome islandBiome in islandBiomes) {
                        if (islandBiome.tiles.Contains(currentTile)) {
                            Biome currentBiome = islandBiome.biome;

                            // Assign terrain color based on the biome's regions
                            for (int i = 0; i < currentBiome.regions.Length; i++) {
                                if (currentHeight <= currentBiome.regions[i].height) {
                                    colourMap[y * mapChunkSize + x] = currentBiome.regions[i].colour;
                                    isBiomeTile = true;
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    // If the tile is not part of any biome, assign water or other color
                    if (!isBiomeTile) {
                        for (int i = 0; i < regions.Length; i++) {
                            if (currentHeight <= regions[i].height) {
                                colourMap[y * mapChunkSize + x] = regions[i].colour;
                                break;
                            }
                        }
                    }
                }
            }
            biomeSpecificHeights = biomeBlendedHeights; // this is set after colors are painted onto map as it effects the color map if heights are overridden before
        } else {
            for (int y = 0; y < mapChunkSize; y++) {
                for (int x = 0; x < mapChunkSize; x++) {
                    float currentHeight = noiseMap[x, y];
                    Vector2Int currentTile = new Vector2Int(x, y);

                    // Check which biome the current tile belongs to and assign the corresponding color
                    bool isBiomeTile = false;
                    
                    foreach (IslandBiome islandBiome in islandBiomes) {
                        if (islandBiome.tiles.Contains(currentTile)) { // if this IslandBiome contains the tile
                            Biome currentBiome = islandBiome.biome;
                            biomeSpecificHeights[x, y] = currentBiome.meshHeightCurve.Evaluate(currentHeight) * currentBiome.meshHeightMultiplier;

                            // Assign terrain color based on the biome's regions
                            for (int i = 0; i < currentBiome.regions.Length; i++)
                            {
                                if (currentHeight <= currentBiome.regions[i].height)
                                {
                                    colourMap[y * mapChunkSize + x] = currentBiome.regions[i].colour;
                                    isBiomeTile = true;
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    // If the tile is not part of any biome, assign water or other color
                    if (!isBiomeTile) {
                        for (int i = 0; i < regions.Length; i++) {
                            if (currentHeight <= regions[i].height) {
                                colourMap[y * mapChunkSize + x] = regions[i].colour;
                                biomeSpecificHeights[x, y] = meshHeightCurve.Evaluate(currentHeight) * meshHeightMultiplier;
                                break;
                            }
                        }
                    }

                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay> ();
        if (drawMode == DrawMode.NoiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        } else if (drawMode == DrawMode.ColourMap) {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh) {
             display.DrawBiomeMesh(noiseMap, biomeSpecificHeights, colourMap, mapChunkSize, mapChunkSize, levelOfDetail, useFlatShading);
        } else if (drawMode == DrawMode.FalloffMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
        }
        PlaceEnvironmentalAssets();
    }

     // Mock function to simulate getting terrain height
    private float GetTerrainHeightAtPosition(Vector3 position)
{
    int x = Mathf.Clamp(Mathf.FloorToInt(position.x), 0, mapChunkSize - 1);
    int y = Mathf.Clamp(Mathf.FloorToInt(position.z), 0, mapChunkSize - 1);

    if (noiseMap == null)
    {
        Debug.LogError("noiseMap is not initialized.");
        return 0f; // Or a suitable default value
    }

    // Return the height from the noise map
    return noiseMap[x, y];
}

    private void PlaceEnvironmentalAssets()
    {
        GameObject environmentParent = GameObject.Find("Environment") ?? new GameObject("Environment");

        GameObject meshObject = GameObject.Find("Mesh");
        MeshCollider meshCollider = meshObject?.GetComponent<MeshCollider>();
        float mapWidth = 100f; // Default width
        float mapDepth = 100f; // Default depth
        float overlapCheckRadius = 2f;

        if (meshObject != null)
        {
            MeshFilter meshFilter = meshObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                // Get the bounds of the mesh in local coordinates
                Bounds meshBounds = meshFilter.sharedMesh.bounds;
                
                // Calculate world dimensions by multiplying bounds size by the scale
                mapWidth = meshBounds.size.x * meshObject.transform.localScale.x;
                mapDepth = meshBounds.size.z * meshObject.transform.localScale.z;
                // Debug.Log("Map Width: " + mapWidth);
                // Debug.Log("Map Depth: " + mapDepth);
            }
        }
        
        foreach (IslandBiome islandBiome in islandBiomes)
        {
            Biome currentBiome = islandBiome.biome;

            foreach (BiomeEnvironmentAssetEntry entry in currentBiome.environment)
            {
                // Locate the Mesh object and retrieve its scale
                Vector3 meshScale = Vector3.one; // Default scale if Mesh is not found

                if (meshObject != null)
                {
                    meshScale = meshObject.transform.localScale; // Get the scale of the mesh
                }
                // Debug.Log("Mesh scale x: " + meshScale.x);
                // Debug.Log("Mesh scale y: " + meshScale.z);

                foreach (Vector2Int tile in islandBiome.tiles)
                {
                    if (UnityEngine.Random.value < entry.frequency) 
                    {
                        // Initial position based on tile coordinates (used for height and region checks)
                        Vector3 position = new Vector3(tile.x, biomeSpecificHeights[tile.x, tile.y], tile.y);

                        // Check if the asset can be placed in this region based on height or other criteria
                        if (IsRegionAllowed(entry.asset, currentBiome, position))
                        {
                            // Calculate the final, adjusted position to account for the mesh scale and centering around origin
                            Vector3 finalPosition = new Vector3(
                                position.x * meshScale.x,
                                position.y, // Keep the original height unchanged
                                -position.z * meshScale.z
                            );

                            // Apply offset to center around the origin
                            //Vector3 offset = new Vector3(-1f * finalPosition.x, 0, -1f * finalPosition.z);
                            finalPosition.x = finalPosition.x - (.5f*mapWidth);
                            finalPosition.z = finalPosition.z + (.5f*mapDepth);
                            

                            // Offset finalPosition.y to be above the mesh before raycasting
                            Vector3 rayOrigin = finalPosition + Vector3.up * 100f;

                            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, Mathf.Infinity, meshCollider ? 1 << meshCollider.gameObject.layer : ~0))
                            {
                                finalPosition.y = hit.point.y - .5f;
                                //Debug.Log("Map Height: " + finalPosition.y);
                            }
                            else
                            {
                                // Debug.LogWarning("Raycast did not hit any collider for position: " + rayOrigin);
                            }
                            
                            // Check if an asset already exists within the overlapCheckRadius
                            if (!Physics.CheckSphere(finalPosition, overlapCheckRadius, LayerMask.GetMask("EnvironmentAssets")))
                            {
                                Quaternion originalRotation = entry.asset.prefab.transform.rotation;
                                float randomY = UnityEngine.Random.Range(0f, 360f);
                                Quaternion finalRotation = Quaternion.Euler(originalRotation.eulerAngles.x, randomY, originalRotation.eulerAngles.z);

                                GameObject instantiatedAsset = Instantiate(entry.asset.prefab, finalPosition, finalRotation, environmentParent.transform);
                                instantiatedAsset.transform.localScale *= 4f;

                                // Optional: Set the new asset to the "EnvironmentAssets" layer
                                instantiatedAsset.layer = LayerMask.NameToLayer("EnvironmentAssets");
                            }
                            else
                            {
                                Debug.Log("Another environmental asset detected at this location. Skipping placement.");
                            }
                        }
                    }
                }
            }
        }
    }

    // Check if the asset can be placed in the current biome's allowed regions
    private bool IsRegionAllowed(BiomeEnvironmentAsset asset, Biome biome, Vector3 position)
    {
        // Here, determine the region based on position
        TerrainType currentRegion = GetTerrainTypeAtPosition(position, biome); // Implement this based on your terrain generation logic

        // Check if the current region is allowed for the asset
        int regionIndex = Array.IndexOf(biome.regions, currentRegion);
        return regionIndex >= 0 && asset.allowedRegions[regionIndex];
    }

    private TerrainType GetTerrainTypeAtPosition(Vector3 position, Biome biome)
    {
        // Assuming you have a method to get the height at the position
        float currentHeight = GetTerrainHeightAtPosition(position);

        // Loop through the biome's regions to find the correct TerrainType
        for (int i = 0; i < biome.regions.Length; i++)
        {
            TerrainType region = biome.regions[i];

            // Check if the current height is less than or equal to the region's maxHeight
            if (currentHeight <= region.height)
            {
                // Return the TerrainType if the height fits within its range
                return region;
            }
        }

        // Return null or a default TerrainType if no match is found
        return null; // or return a default TerrainType if necessary
    }

    // Simple method to check if a tile is a border tile (customize this to your needs)
    bool IsBorderTile(Vector2Int tile, HashSet<Vector2Int> biomeArea, float[,] noiseMap)
    {
        // Check the surrounding tiles
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int dir in directions)
        {
            Vector2Int adjacentTile = tile + dir;
            if (!biomeArea.Contains(adjacentTile))
            {
                return true; // This tile is on the edge of the biome
            }
        }
        return false;
    }

    void OnValidate() {
        if (lacunarity < 1) {
            lacunarity = 1;
        }
        if (octaves < 0) {
            octaves = 0;
        }

        falloffMap = FalloffGenerator.GenerateFalloffMap (mapChunkSize);
    }

    void CleanUpPrevMapGeneration()
    {
        // Find all IslandBiome components attached to any GameObjects in the scene
        IslandBiome[] existingBiomes = FindObjectsOfType<IslandBiome>();

        foreach (IslandBiome biome in existingBiomes)
        {
            // Use DestroyImmediate if we're in the editor (edit mode), otherwise use Destroy
            if (Application.isEditor && !Application.isPlaying)
            {
                DestroyImmediate(biome);
            }
            else
            {
                Destroy(biome);
            }
        }

        // Find the Environment GameObject
        GameObject environmentParent = GameObject.Find("Environment");
        if (environmentParent != null)
        {
            // Collect all child GameObjects in a list
            List<GameObject> childrenToDestroy = new List<GameObject>();

            foreach (Transform child in environmentParent.transform)
            {
                childrenToDestroy.Add(child.gameObject);
            }

            // Now destroy the collected children
            foreach (GameObject child in childrenToDestroy)
            {
                if (Application.isEditor && !Application.isPlaying)
                {
                    DestroyImmediate(child);
                }
                else
                {
                    Destroy(child);
                }
            }
        }
    }

}