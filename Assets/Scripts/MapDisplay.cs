using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void DrawTexture(Texture2D texture) {
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData, Texture2D texture) {
        meshFilter.sharedMesh = meshData.CreateMesh ();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }

    public void DrawBiomeMesh(float[,] noiseMap, float[,] biomeSpecificHeights, Color[] colourMap, int width, int height, int levelOfDetail) {
        // Generate the mesh using biome-specific height data
        MeshData biomeMeshData = MeshGenerator.GenerateTerrainMesh(noiseMap, biomeSpecificHeights, levelOfDetail);

        // Apply the generated mesh and color texture to the terrain
        meshFilter.sharedMesh = biomeMeshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = TextureGenerator.TextureFromColourMap(colourMap, width, height);
    }
}
