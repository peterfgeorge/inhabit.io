using UnityEngine;

[System.Serializable]
public class Biome : MonoBehaviour
{
    public string biomeName;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    public TerrainType[] regions; // Array of TerrainType specific to each biome
}