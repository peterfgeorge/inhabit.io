using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandGenUIHandler : MonoBehaviour
{
    public GameObject mapGeneratorObject;
    MapGenerator mapGenerator;
    // Start is called before the first frame update
    void Start()
    {
        if (mapGeneratorObject != null)
        {
            mapGenerator = mapGeneratorObject.GetComponent<MapGenerator>();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerateIsland()
    {
        mapGenerator.GenerateMap();
        Debug.Log(mapGenerator.biomes);
    }
}
