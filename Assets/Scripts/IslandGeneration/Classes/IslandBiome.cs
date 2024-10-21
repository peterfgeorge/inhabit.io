using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IslandBiome {
    public Biome biome; // Reference to the biome
    public HashSet<Vector2Int> tiles; // All tiles in the biome
    public HashSet<Vector2Int> borderTiles; // Border tiles for the biome

    public IslandBiome(Biome biome) {
        this.biome = biome;
        tiles = new HashSet<Vector2Int>();
        borderTiles = new HashSet<Vector2Int>();
    }
}
