using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class IslandBiome : MonoBehaviour {
    public Biome biome; // Reference to the biome
    public HashSet<Vector2Int> tiles; // All tiles in the biome
    public HashSet<Vector2Int> borderTiles; // Border tiles for the biome

    public void SetBiome(Biome newBiome)
    {
        biome = newBiome;
        tiles = new HashSet<Vector2Int>();
        borderTiles = new HashSet<Vector2Int>();
    }
}
