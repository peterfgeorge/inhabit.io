using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public Mesh GetMesh() {
        return GetComponent<MeshFilter>().sharedMesh;
    }

    public void DrawTexture(Texture2D texture) {
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData, Texture2D texture) {
        meshFilter.sharedMesh = meshData.CreateMesh ();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }

    public void DrawBiomeMesh(float[,] noiseMap, float[,] biomeSpecificHeights, Color[] colourMap, int width, int height, int levelOfDetail, bool useFlatShading) {
        // Generate the mesh using biome-specific height data
        MeshData biomeMeshData = MeshGenerator.GenerateTerrainMesh(noiseMap, biomeSpecificHeights, levelOfDetail, useFlatShading);

        // Apply the generated mesh and color texture to the terrain
        Mesh generatedMesh = biomeMeshData.CreateMesh();
        meshFilter.sharedMesh = generatedMesh;
        meshRenderer.sharedMaterial.mainTexture = TextureGenerator.TextureFromColourMap(colourMap, width, height);

        // Find the "Mesh" GameObject
        GameObject meshGameObject = GameObject.Find("Mesh");
        if (meshGameObject != null) {
            // Get the MeshCollider component from the "Mesh" GameObject
            MeshCollider meshCollider = meshGameObject.GetComponent<MeshCollider>();
            if (meshCollider != null) {
                meshCollider.sharedMesh = generatedMesh; // Update the MeshCollider with the new mesh
            } else {
                Debug.LogError("MeshCollider component is missing on the 'Mesh' GameObject.");
            }
        } else {
            Debug.LogError("Mesh GameObject not found in the scene. Make sure it is named 'Mesh'.");
        }
    }
}
