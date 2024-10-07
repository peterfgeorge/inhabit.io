using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class IslandRemover {
    public static float[,] RemoveSmallerIslands(float[,] noiseMap, float landThreshold) {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        bool[,] visited = new bool[width, height];
        List<List<Vector2Int>> islands = new List<List<Vector2Int>>();

        // Flood fill to find all islands
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                if (!visited[x, y] && noiseMap[x, y] > landThreshold) {
                    List<Vector2Int> island = FloodFill(noiseMap, visited, x, y, landThreshold);
                    islands.Add(island);
                }
            }
        }

        // Find the largest island
        List<Vector2Int> largestIsland = islands.OrderByDescending(island => island.Count).FirstOrDefault();

        // Remove all smaller islands
        foreach (List<Vector2Int> island in islands) {
            if (island != largestIsland) {
                foreach (Vector2Int tile in island) {
                    noiseMap[tile.x, tile.y] = 0.35f; // Set smaller island tiles to shallow water (height = .35)
                }
            }
        }

        return noiseMap;
    }

    private static List<Vector2Int> FloodFill(float[,] noiseMap, bool[,] visited, int startX, int startY, float landThreshold) {
        List<Vector2Int> islandTiles = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0) {
            Vector2Int tile = queue.Dequeue();
            islandTiles.Add(tile);

            foreach (Vector2Int neighbor in GetNeighbors(tile, noiseMap.GetLength(0), noiseMap.GetLength(1))) {
                if (!visited[neighbor.x, neighbor.y] && noiseMap[neighbor.x, neighbor.y] > landThreshold) {
                    visited[neighbor.x, neighbor.y] = true;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return islandTiles;
    }

    private static List<Vector2Int> GetNeighbors(Vector2Int tile, int width, int height) {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        if (tile.x > 0) neighbors.Add(new Vector2Int(tile.x - 1, tile.y)); // Left
        if (tile.x < width - 1) neighbors.Add(new Vector2Int(tile.x + 1, tile.y)); // Right
        if (tile.y > 0) neighbors.Add(new Vector2Int(tile.x, tile.y - 1)); // Down
        if (tile.y < height - 1) neighbors.Add(new Vector2Int(tile.x, tile.y + 1)); // Up

        return neighbors;
    }
}