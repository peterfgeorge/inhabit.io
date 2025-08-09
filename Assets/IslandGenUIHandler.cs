using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandGenUIHandler : MonoBehaviour
{
    public GameObject mapGeneratorObject;
    MapGenerator mapGenerator;
    public GameObject biomeList;
    public GameObject biomeEntryPrefab;
    public TMPro.TextMeshProUGUI numberOfBiomesText;
    public bool randomSeed = true;
    public TMPro.TMP_InputField seedInputField;
    // Start is called before the first frame update
    void Start()
    {
        if (mapGeneratorObject != null)
        {
            mapGenerator = mapGeneratorObject.GetComponent<MapGenerator>();
        }
        PopulateBiomeList();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerateIsland()
    {
        SetNumberOfBiomes(int.Parse(numberOfBiomesText.text));
        SetSeed();
        mapGenerator.GenerateMap();
    }

    public void PopulateBiomeList()
    {
        foreach (var biome in mapGenerator.biomes)
        {
            GameObject entry = Instantiate(biomeEntryPrefab, biomeList.transform);
            entry.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = biome.name;
            // entry.GetComponent<Button>().onClick.AddListener(() => OnBiomeSelected(biome));
        }
    }

    public void SetNumberOfBiomes(int number)
    {
        mapGenerator.numberOfBiomes = number;
    }

    public void SetSeed()
    {
        if (randomSeed)
        {
            mapGenerator.seed = Random.Range(0, int.MaxValue);
            seedInputField.text = mapGenerator.seed.ToString();
        }
        else
        {
            mapGenerator.seed = int.Parse(seedInputField.text);
        }
    }
}
