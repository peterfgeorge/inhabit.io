using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]  // Ensures the script runs in the editor
public class PlaneTilerEditor : MonoBehaviour
{
    public int gridWidth = 10;  // Number of planes along the x-axis
    public int gridHeight = 10; // Number of planes along the z-axis
    public GameObject planePrefab;  // Assign a 1x1 plane prefab in the inspector
    public bool generateGrid = false;  // Button to trigger grid generation

    private void Update()
    {
        // Check if 'generateGrid' is toggled on in the editor
        if (generateGrid)
        {
            GenerateGrid();
            generateGrid = false;  // Reset to prevent continuous generation
        }
    }

    public void GenerateGrid()
    {
        // Clear any existing child objects to start with a fresh grid
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        // Loop to instantiate planes in a grid pattern
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 position = new Vector3(x*200 - 1460, 19, z*200 - 1418);  // Position for each plane
                GameObject plane = (GameObject)PrefabUtility.InstantiatePrefab(planePrefab, transform);
                plane.transform.position = position;
                plane.transform.SetParent(transform);  // Make it a child of the object with this script
            }
        }
    }
}
