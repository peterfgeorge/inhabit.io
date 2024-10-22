using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{
    // Modified method to accept biome-specific heights
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float[,] biomeSpecificHeights, int levelOfDetail, bool useFlatShading) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine, useFlatShading);
        int vertexIndex = 0;

        for (int y = 0; y < height; y += meshSimplificationIncrement) {
            for (int x = 0; x < width; x += meshSimplificationIncrement) {
                // Apply biome-specific height for each vertex
                meshData.vertices[vertexIndex] = new Vector3(
                    topLeftX + x,
                    biomeSpecificHeights[x, y],  // Use biome-specific height here
                    topLeftZ - y
                );

                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                // Add triangles only if within bounds
                if (x < width - meshSimplificationIncrement && y < height - meshSimplificationIncrement) {
                    int a = vertexIndex;
                    int b = vertexIndex + verticesPerLine + 1;
                    int c = vertexIndex + verticesPerLine;
                    int d = vertexIndex + 1;

                    meshData.AddTriangle(a, b, c);
                    meshData.AddTriangle(b, a, d);
                }

                vertexIndex++;
            }
        }


        meshData.ProcessMesh();
        return meshData;
    }
}

public class MeshData {
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex;

    bool useFlatShading;

    public MeshData(int meshWidth, int meshHeight, bool useFlatShading) {
        this.useFlatShading = useFlatShading;
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c) {
        triangles [triangleIndex] = a;
        triangles [triangleIndex+1] = b;
        triangles [triangleIndex+2] = c;
        triangleIndex += 3;
    }

    public void ProcessMesh() {
        if (useFlatShading) {
            FlatShading();
        }
    }

    void FlatShading() {
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];
        for (int i=0; i<triangles.Length; i++) {
            flatShadedVertices [i] = vertices [triangles [i]];
            flatShadedUvs [i] = uvs [triangles [i]];
            triangles [i] = i;
        }

        vertices = flatShadedVertices;
        uvs = flatShadedUvs;
    }

    public Mesh CreateMesh() {
        Mesh mesh = new Mesh ();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}