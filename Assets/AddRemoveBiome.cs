using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddRemoveBiome : MonoBehaviour
{
    private GameObject UI;
    IslandGenUIHandler UIHandler;
    private GameObject mapGeneratorObject;
    MapGenerator mapGenerator;

    void Awake()
    {
        UI = GameObject.Find("UI");
        mapGeneratorObject = GameObject.Find("MapGenerator");
        UIHandler = UI.GetComponent<IslandGenUIHandler>();
        mapGenerator = mapGeneratorObject.GetComponent<MapGenerator>();
    }

    public void AddBiomeFromUIText()
    {
        var textComponent = gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        string biomeName = textComponent.text;
        Biome biome = FindBiomeByName(biomeName);
        UIHandler.AddBiomeToList(biome);
    }

    public void RemoveBiomeFromUIText()
    {
        var textComponent = gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        string biomeName = textComponent.text;
        Biome biome = FindBiomeByName(biomeName);
        UIHandler.RemoveBiomeFromList(biome);
    }

    private Biome FindBiomeByName(string name)
    {
        foreach (Biome b in mapGenerator.everyBiomePrefab)
        {
            if (b.name == name)
                return b;
        }
        return null;
    }
}
