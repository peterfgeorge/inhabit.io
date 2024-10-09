using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BiomeGenerator
{
    public static List<HashSet<Vector2Int>> DivideMainIslandIntoBiomes(List<Vector2Int> mainIsland, int numberOfBiomes)
    {
        List<HashSet<Vector2Int>> biomes = new List<HashSet<Vector2Int>>();
        HashSet<Vector2Int> unassignedTiles = new HashSet<Vector2Int>(mainIsland);
        int totalArea = mainIsland.Count;
        List<int> targetAreas = new List<int>();

        // Step 1: Determine the target size for each biome
        for (int i = 0; i < numberOfBiomes; i++)
        {
            int minBiomeSize = totalArea / (numberOfBiomes * 2);
            int maxBiomeSize = totalArea / numberOfBiomes;
            int area = Random.Range(minBiomeSize, maxBiomeSize);
            targetAreas.Add(area);
        }

        // Step 2: Create the biomes using flood fill
        for (int i = 0; i < numberOfBiomes; i++)
        {
            HashSet<Vector2Int> biome = new HashSet<Vector2Int>();

            if (unassignedTiles.Count == 0)
                break;

            Vector2Int startTile = PickRandomTile(unassignedTiles);
            biome.Add(startTile);
            unassignedTiles.Remove(startTile);

            Queue<Vector2Int> frontier = new Queue<Vector2Int>();
            frontier.Enqueue(startTile);

            while (biome.Count < targetAreas[i] && frontier.Count > 0)
            {
                Vector2Int currentTile = frontier.Dequeue();
                List<Vector2Int> neighbors = GetNeighbors(currentTile, unassignedTiles);
                neighbors = neighbors.OrderBy(x => UnityEngine.Random.value).ToList();

                foreach (Vector2Int neighbor in neighbors)
                {
                    if (!biome.Contains(neighbor))
                    {
                        biome.Add(neighbor);
                        unassignedTiles.Remove(neighbor);
                        frontier.Enqueue(neighbor);
                    }

                    if (biome.Count >= targetAreas[i])
                        break;
                }
            }

            biomes.Add(biome);
        }

        // Step 3: Fill unassigned tiles with the closest biome
        FillUnassignedTilesWithClosestBiome(unassignedTiles, biomes);

        return biomes;
    }

    // Step 3: Method to fill remaining unassigned tiles with the closest biome
    private static void FillUnassignedTilesWithClosestBiome(HashSet<Vector2Int> unassignedTiles, List<HashSet<Vector2Int>> biomes)
    {
        while (unassignedTiles.Count > 0)
        {
            Vector2Int unassignedTile = unassignedTiles.First();
            float minDistance = float.MaxValue;
            HashSet<Vector2Int> closestBiome = null;

            // Find the closest biome
            foreach (var biome in biomes)
            {
                foreach (var biomeTile in biome)
                {
                    float distance = Vector2Int.Distance(unassignedTile, biomeTile);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestBiome = biome;
                    }
                }
            }

            // Assign the unassigned tile to the closest biome
            if (closestBiome != null)
            {
                closestBiome.Add(unassignedTile);
            }

            unassignedTiles.Remove(unassignedTile);
        }
    }

    private static List<Vector2Int> GetNeighbors(Vector2Int tile, HashSet<Vector2Int> unassignedTiles)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        Vector2Int[] directions = {
            new Vector2Int(0, 1),  // Up
            new Vector2Int(0, -1), // Down
            new Vector2Int(1, 0),  // Right
            new Vector2Int(-1, 0)  // Left
        };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighbor = tile + direction;
            if (unassignedTiles.Contains(neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private static Vector2Int PickRandomTile(HashSet<Vector2Int> unassignedTiles)
    {
        return unassignedTiles.ElementAt(UnityEngine.Random.Range(0, unassignedTiles.Count));
    }
}
