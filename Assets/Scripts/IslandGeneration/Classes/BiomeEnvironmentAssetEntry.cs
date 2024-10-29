using UnityEngine;

[System.Serializable]
public class BiomeEnvironmentAssetEntry
{
    public BiomeEnvironmentAsset asset;
    [Range(0f, 1f)] 
    public float frequency = 0.5f;
}