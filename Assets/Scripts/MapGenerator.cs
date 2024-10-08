using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {NoiseMap, ColourMap, Mesh, FalloffMap}
    public DrawMode drawMode;

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

        // Update the colourMap based on the modified noiseMap
        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++) {
            for (int x = 0; x < mapChunkSize; x++) {
                float currentHeight = noiseMap[x, y];

                // Update color based on the modified height values (including removed islands)
                for (int i = 0; i < regions.Length; i++) {
                    if (currentHeight <= regions[i].height) {
                        colourMap[y * mapChunkSize + x] = regions[i].colour;
                        break;
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
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
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