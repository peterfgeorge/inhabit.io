using System.Collections.Generic;
using UnityEngine;

public class BlendBiomes
{
    private List<IslandBiome> islandBiomes;
    private float[,] noiseMap;

    public BlendBiomes(List<IslandBiome> islandBiomes, float[,] noiseMap)
    {
        this.islandBiomes = islandBiomes;
        this.noiseMap = noiseMap;
    }

    public void BlendBiomeHeights(float[,] biomeSpecificHeights)
    {
        // Create a distance map for each biome
        Dictionary<IslandBiome, float[,]> distanceMaps = GenerateDistanceMaps();

        // For each tile, calculate the blended height based on distances
        for (int y = 0; y < biomeSpecificHeights.GetLength(1); y++)
        {
            for (int x = 0; x < biomeSpecificHeights.GetLength(0); x++)
            {
                float blendedHeight = 0;
                float totalWeight = 0;

                // Iterate through all biomes to calculate blended height
                foreach (IslandBiome islandBiome in islandBiomes)
                {
                    float distance = distanceMaps[islandBiome][x, y];
                    float weight = Mathf.Clamp(1f / Mathf.Pow(distance + 1f, 2f), 0f, 1f); // Clamp max weight to avoid dominance
                    float biomeHeight = islandBiome.biome.meshHeightCurve.Evaluate(noiseMap[x, y]) * islandBiome.biome.meshHeightMultiplier;

                    blendedHeight += biomeHeight * weight;
                    totalWeight += weight;
                }

                // Normalize the blended height
                if (totalWeight > 0)
                {
                    biomeSpecificHeights[x, y] = blendedHeight / totalWeight;
                }
                else
                {
                    biomeSpecificHeights[x, y] = 0; // or another default value
                }
            }
        }
    }

    private Dictionary<IslandBiome, float[,]> GenerateDistanceMaps()
    {
        // Create a distance map for each biome
        Dictionary<IslandBiome, float[,]> distanceMaps = new Dictionary<IslandBiome, float[,]>();

        foreach (IslandBiome islandBiome in islandBiomes)
        {
            float[,] distanceMap = new float[MapGenerator.mapChunkSize, MapGenerator.mapChunkSize];

            for (int y = 0; y < MapGenerator.mapChunkSize; y++)
            {
                for (int x = 0; x < MapGenerator.mapChunkSize; x++)
                {
                    distanceMap[x, y] = CalculateDistanceToBorder(new Vector2Int(x, y), islandBiome);
                }
            }

            distanceMaps[islandBiome] = distanceMap;
        }

        return distanceMaps;
    }

    private float CalculateDistanceToBorder(Vector2Int tile, IslandBiome biome)
    {
        float minDistance = float.MaxValue;

        foreach (Vector2Int borderTile in biome.borderTiles)
        {
            float distance = Vector2Int.Distance(tile, borderTile);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        return minDistance;
    }
}
