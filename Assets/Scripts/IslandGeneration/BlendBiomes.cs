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
        Dictionary<Vector2Int, List<Biome>> nearbyBiomes = GetNearbyBiomes();

        for (int y = 0; y < biomeSpecificHeights.GetLength(1); y++)
        {
            for (int x = 0; x < biomeSpecificHeights.GetLength(0); x++)
            {
                Vector2Int currentTile = new Vector2Int(x, y);
                if (nearbyBiomes.ContainsKey(currentTile))
                {
                    float totalWeight = 0;
                    float blendedHeight = 0;

                    // Influence from the current biome
                    Biome currentBiome = GetBiomeForTile(currentTile);
                    float currentBiomeHeight = currentBiome.meshHeightCurve.Evaluate(noiseMap[x, y]) * currentBiome.meshHeightMultiplier;
                    blendedHeight += currentBiomeHeight * 0.8f; // Default influence

                    // Influence from nearby biomes
                    foreach (Biome neighborBiome in nearbyBiomes[currentTile])
                    {
                        float neighborHeight = neighborBiome.meshHeightCurve.Evaluate(noiseMap[x, y]) * neighborBiome.meshHeightMultiplier;
                        float distance = Vector2Int.Distance(currentTile, currentTile); // Calculate the distance
                        float weight = Mathf.Max(0, 1 - (distance / 5f)); // Weight based on distance
                        blendedHeight += neighborHeight * weight;
                        totalWeight += weight;
                    }

                    // Normalize blended height
                    if (totalWeight > 0)
                    {
                        blendedHeight /= (1 + totalWeight); // Normalize by total weights
                    }
                    biomeSpecificHeights[x, y] = blendedHeight;
                }
            }
        }
    }

    private Dictionary<Vector2Int, List<Biome>> GetNearbyBiomes()
    {
        Dictionary<Vector2Int, List<Biome>> nearbyBiomes = new Dictionary<Vector2Int, List<Biome>>();

        foreach (IslandBiome islandBiome in islandBiomes)
        {
            foreach (Vector2Int borderTile in islandBiome.borderTiles)
            {
                // Check neighboring tiles within a 5-tile radius
                for (int y = -5; y <= 5; y++)
                {
                    for (int x = -5; x <= 5; x++)
                    {
                        if (Mathf.Abs(x) + Mathf.Abs(y) <= 5) // Ensure it's within a radius
                        {
                            Vector2Int neighborTile = borderTile + new Vector2Int(x, y);
                            if (neighborTile.x >= 0 && neighborTile.x < MapGenerator.mapChunkSize && neighborTile.y >= 0 && neighborTile.y < MapGenerator.mapChunkSize)
                            {
                                // Check if the neighbor is part of a different biome
                                foreach (IslandBiome otherBiome in islandBiomes)
                                {
                                    if (otherBiome != islandBiome && otherBiome.tiles.Contains(neighborTile))
                                    {
                                        if (!nearbyBiomes.ContainsKey(borderTile))
                                        {
                                            nearbyBiomes[borderTile] = new List<Biome>();
                                        }
                                        if (!nearbyBiomes[borderTile].Contains(otherBiome.biome))
                                        {
                                            nearbyBiomes[borderTile].Add(otherBiome.biome);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return nearbyBiomes;
    }

    private Biome GetBiomeForTile(Vector2Int tile)
    {
        foreach (IslandBiome islandBiome in islandBiomes)
        {
            if (islandBiome.tiles.Contains(tile))
            {
                return islandBiome.biome;
            }
        }
        return null; // or return a default biome if necessary
    }
}
