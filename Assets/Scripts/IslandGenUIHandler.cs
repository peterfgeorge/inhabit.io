using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IslandGenUIHandler : MonoBehaviour
{
    public GameObject mapGeneratorObject;
    MapGenerator mapGenerator;
    public GameObject biomeList;
    public GameObject biomeEditList;

    public GameObject biomeEntryPrefab;
    public GameObject biomeEditListEntryPrefab;
    public TMPro.TextMeshProUGUI numberOfBiomesText;
    public bool randomSeed = true;
    public TMPro.TMP_InputField seedInputField;
    public List<Biome> tempBiomeSelectionList;
    public List<Biome> tempSelectedBiomesList;
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

    public void PopulateBiomeEditList()
    {
        // Remove old children
        foreach (Transform child in biomeEditList.transform)
        {
            Destroy(child.gameObject);
        }
        tempSelectedBiomesList = new List<Biome>();
        tempBiomeSelectionList = new List<Biome>(mapGenerator.everyBiomePrefab);
        foreach (var biome in tempBiomeSelectionList)
        {
            GameObject entry = Instantiate(biomeEditListEntryPrefab, biomeEditList.transform);
            entry.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = biome.name;

            if (mapGenerator.biomes.Contains(biome))
            {
                entry.transform.Find("Check Handler/Active").gameObject.SetActive(true);
                entry.transform.Find("Check Handler/Inactive").gameObject.SetActive(false);
                tempSelectedBiomesList.Add(biome);
                Debug.Log("Added " + biome.name + " to selected biomes list.");
            }
        }
        Debug.Log("Populated Biome Edit List with " + tempBiomeSelectionList.Count + " biomes.");
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
            if (string.IsNullOrEmpty(seedInputField.text))
            {
                seedInputField.text = Random.Range(0, int.MaxValue).ToString();
            }
            mapGenerator.seed = int.Parse(seedInputField.text);
        }
    }

    public void ToggleRandomSeed()
    {
        if (randomSeed)
        {
            randomSeed = false;
        }
        else
        {
            randomSeed = true;
        }
    }

    public void AddBiomeToList(Biome biome)
    {
        tempSelectedBiomesList.Add(biome);
        Debug.Log("Populated Biome Edit List with " + tempSelectedBiomesList.Count + " biomes.");
    }
    public void RemoveBiomeFromList(Biome biome)
    {
        tempSelectedBiomesList.Remove(biome);
        Debug.Log("Populated Biome Edit List with " + tempSelectedBiomesList.Count + " biomes.");
    }

    public void SetBiomeList()
    {
        foreach (Transform child in biomeList.transform)
        {
            Debug.Log("Destroying child: " + child.name);
            Destroy(child.gameObject);
        }

        mapGenerator.biomes = tempSelectedBiomesList;
        PopulateBiomeList();
    }
}
