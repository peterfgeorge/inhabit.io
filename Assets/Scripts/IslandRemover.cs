using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class IslandRemover {
    public static List<Vector2Int> GetMainIslandCoordinates(float[,] noiseMap, float landThreshold) {
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

        // Find the largest island and return its coordinates
        List<Vector2Int> largestIsland = islands.OrderByDescending(island => island.Count).FirstOrDefault();

        return largestIsland;
    }
    public static float[,] RemoveSmallerIslands(float[,] noiseMap, float landThreshold, int bufferRange) {
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

        // Set height to zero around the largest island
        if (largestIsland != null && largestIsland.Count > 0) {
            noiseMap = SetHeightToZeroAroundIsland(noiseMap, largestIsland, bufferRange);
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

    public static float[,] SetHeightToZeroAroundIsland(float[,] noiseMap, List<Vector2Int> mainIsland, float distanceThreshold) {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        // Create a HashSet for faster lookups
        HashSet<Vector2Int> islandSet = new HashSet<Vector2Int>(mainIsland);

        // Calculate the square of the distance threshold
        float distanceThresholdSquared = distanceThreshold * distanceThreshold;

        // Define grid cell size based on the distance threshold (e.g., the length of one side of a partition)
        int cellSize = Mathf.CeilToInt(distanceThreshold);

        // Create a dictionary to hold island tiles by grid cell
        Dictionary<Vector2Int, List<Vector2Int>> grid = new Dictionary<Vector2Int, List<Vector2Int>>();

        // Populate the grid with island tiles
        foreach (Vector2Int islandTile in mainIsland) {
            Vector2Int gridPos = new Vector2Int(islandTile.x / cellSize, islandTile.y / cellSize);
            if (!grid.ContainsKey(gridPos)) {
                grid[gridPos] = new List<Vector2Int>();
            }
            grid[gridPos].Add(islandTile);
        }

        // Iterate over every tile in the noise map
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                Vector2Int currentTile = new Vector2Int(x, y);
                Vector2Int gridPos = new Vector2Int(currentTile.x / cellSize, currentTile.y / cellSize);

                bool isNearIsland = false;

                // Check neighboring grid cells (including the current cell and its neighbors)
                for (int offsetY = -1; offsetY <= 1; offsetY++) {
                    for (int offsetX = -1; offsetX <= 1; offsetX++) {
                        Vector2Int neighborGridPos = new Vector2Int(gridPos.x + offsetX, gridPos.y + offsetY);
                        if (grid.ContainsKey(neighborGridPos)) {
                            foreach (Vector2Int islandTile in grid[neighborGridPos]) {
                                float distSquared = (currentTile.x - islandTile.x) * (currentTile.x - islandTile.x) +
                                                    (currentTile.y - islandTile.y) * (currentTile.y - islandTile.y);
                                if (distSquared <= distanceThresholdSquared) {
                                    isNearIsland = true;
                                    break;
                                }
                            }
                        }
                        if (isNearIsland) break;
                    }
                    if (isNearIsland) break;
                }

                // If the tile is not near the main island, set it to zero
                if (!isNearIsland) {
                    noiseMap[x, y] = 0f; // Set height to 0 for water
                }
            }
        }

        return noiseMap;
    }

}
