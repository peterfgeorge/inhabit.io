using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {NoiseMap, ColourMap, Mesh, FalloffMap}
    public DrawMode drawMode;
    public List<Biome> biomes;
    const int mapChunkSize = 241;
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
    [Range(0,8)]
    public int numberOfBiomes;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
 
    public bool autoUpdate;

    public TerrainType[] regions;
    float[,] falloffMap;

    void Awake() {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }
    public void GenerateMap() {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

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

        List<HashSet<Vector2Int>> biomeAreas  = new List<HashSet<Vector2Int>>();

        if (addBiomes && biomes.Count > 0) // Check if biomes should be added
        {
            biomeAreas = BiomeGenerator.DivideMainIslandIntoBiomes(mainIsland, numberOfBiomes);
        }

        // Update the colourMap based on the modified noiseMap
        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        float[,] biomeSpecificHeights = new float[mapChunkSize, mapChunkSize];

        int[] biomeAssignments = new int[biomeAreas.Count];

        for(int i=0; i < biomeAreas.Count; i++) {
            biomeAssignments[i] = Random.Range(0, biomes.Count);
        }

        for (int y = 0; y < mapChunkSize; y++) {
            for (int x = 0; x < mapChunkSize; x++) {
                float currentHeight = noiseMap[x, y];
                Vector2Int currentTile = new Vector2Int(x, y);

                // Check which biome the current tile belongs to and assign the corresponding color
                bool isBiomeTile = false;
                
                for (int biomeIndex = 0; biomeIndex < biomeAreas.Count; biomeIndex++) {
                    if (biomeAreas[biomeIndex].Contains(currentTile)) { // if this one area contains that tile
                        Biome currentBiome = biomes[biomeAssignments[biomeIndex]];
                        // Apply biome-specific mesh height multiplier and curve
                        biomeSpecificHeights[x, y] = currentBiome.meshHeightCurve.Evaluate(currentHeight) * currentBiome.meshHeightMultiplier;

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
                            biomeSpecificHeights[x, y] = meshHeightCurve.Evaluate(currentHeight) * meshHeightMultiplier;
                            break;
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
             display.DrawBiomeMesh(noiseMap, biomeSpecificHeights, colourMap, mapChunkSize, mapChunkSize, levelOfDetail);
        } else if (drawMode == DrawMode.FalloffMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
        }
        
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
}

[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color colour;
}

[System.Serializable]
public class Biome
{
    public string biomeName;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    public TerrainType[] regions; // Array of TerrainType specific to each biome
}