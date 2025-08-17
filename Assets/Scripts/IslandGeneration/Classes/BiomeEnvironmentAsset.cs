using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;


[Serializable]
[CreateAssetMenu(fileName = "NewBiomeEnvironmentAsset", menuName = "Environment/BiomeEnvironmentAsset")]
public class BiomeEnvironmentAsset : ScriptableObject
{
    public GameObject prefab; // Prefab for the environment asset

    // Dropdown for selecting biome name
    [HideInInspector]
    public string biomeName;

    public bool scaleUp = true;

    // Boolean array to manage allowed regions for this asset
    public bool[] allowedRegions;

    

    // Method to populate allowedRegions based on the selected biome's regions
    public void InitializeAllowedRegions(TerrainType[] biomeRegions)
    {
        allowedRegions = new bool[biomeRegions.Length];
    }

    // Calculate allowed heights based on the biome's regions
    public float[] CalculateAllowedHeights(Biome biome)
    {
        List<float> allowedHeights = new List<float>();

        for (int i = 0; i < biome.regions.Length; i++)
        {
            TerrainType currentRegion = biome.regions[i];
            Debug.Log("CURRENT REGION: " + biome.regions[i]);

            // Determine the min height based on the previous region
            float minHeight = (i == 0) ? 0 : biome.regions[i - 1].height; // 0 if it's the first region
            float maxHeight = currentRegion.height;

            // Add all heights in the allowed range to the list
            for (float h = minHeight; h <= maxHeight; h += 0.01f) // Increment step can be adjusted
            {
                if (!allowedHeights.Contains(h))
                {
                    allowedHeights.Add(h);
                }
            }
        }

        return allowedHeights.ToArray();
    }
}

#if UNITY_EDITOR

    // Custom Editor to create the dropdown and checkbox UI in the Inspector
    [CustomEditor(typeof(BiomeEnvironmentAsset))]
    public class BiomeEnvironmentAssetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            BiomeEnvironmentAsset asset = (BiomeEnvironmentAsset)target;

            // Dropdown for biome selection from MapGenerator's biomes array
            MapGenerator mapGen = FindObjectOfType<MapGenerator>(); // Ensure there's only one MapGenerator in the scene
            if (mapGen != null)
            {
                string[] biomeNames = new string[mapGen.biomes.Count];
                for (int i = 0; i < biomeNames.Length; i++)
                {
                    biomeNames[i] = mapGen.biomes[i].biomeName;
                }

                // Select biome from dropdown
                int selectedBiomeIndex = EditorGUILayout.Popup("Biome", Array.IndexOf(biomeNames, asset.biomeName), biomeNames);
                if (selectedBiomeIndex >= 0)
                {
                    asset.biomeName = biomeNames[selectedBiomeIndex];

                    // Initialize allowedRegions based on the selected biome's regions
                    TerrainType[] selectedBiomeRegions = mapGen.biomes[selectedBiomeIndex].regions;
                    if (asset.allowedRegions == null || asset.allowedRegions.Length != selectedBiomeRegions.Length)
                    {
                        asset.InitializeAllowedRegions(selectedBiomeRegions);
                    }
                    
                    asset.scaleUp = EditorGUILayout.Toggle("Scale Up", asset.scaleUp);

                    // Display checkboxes for each region
                EditorGUILayout.LabelField("Allowed Regions", EditorStyles.boldLabel);
                    for (int i = 0; i < selectedBiomeRegions.Length; i++)
                    {
                        asset.allowedRegions[i] = EditorGUILayout.Toggle(selectedBiomeRegions[i].name, asset.allowedRegions[i]);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("MapGenerator not found in the scene.", MessageType.Warning);
            }

            // Show prefab field
            asset.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", asset.prefab, typeof(GameObject), false);

            // Mark changes as dirty
            if (GUI.changed)
            {
                EditorUtility.SetDirty(asset);
            }
        }
    }
#endif
