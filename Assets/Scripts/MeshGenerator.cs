using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail)
    {
        AnimationCurve _heightCurve = new AnimationCurve(heightCurve.keys);

        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;

        int borderedSize = heightMap.GetLength(0);
        int meshSize = borderedSize - 2 * meshSimplificationIncrement;
        int meshSizeUnsimplified = borderedSize - 2;

        float topLeftX = (meshSizeUnsimplified - 1) / -2f;
        float topLeftZ = (meshSizeUnsimplified - 1) / 2f;

        int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine);

        int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;

        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;

                if(isBorderVertex)
                {
                    vertexIndicesMap[x, y] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }

            }
        }

        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                int vertexIndex = vertexIndicesMap[x, y];

                Vector2 percent = new Vector2((x - meshSimplificationIncrement) / (float)meshSize, (y - meshSimplificationIncrement) / (float)meshSize);

                float height = _heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;
                Vector3 vertPosition = new Vector3 (topLeftX + percent.x * meshSizeUnsimplified, height, topLeftZ - percent.y * meshSizeUnsimplified);

                meshData.AddVertex(vertPosition, percent, vertexIndex);

                if (x < borderedSize - 1 && y < borderedSize - 1)
                {
                    int a = vertexIndicesMap[x, y];
                    int b = vertexIndicesMap[x + meshSimplificationIncrement, y];
                    int c = vertexIndicesMap[x, y + meshSimplificationIncrement];
                    int d = vertexIndicesMap[x + meshSimplificationIncrement, y + meshSimplificationIncrement];

                    meshData.AddTriangle(a, d, c);
                    meshData.AddTriangle(d, a, b);
                }

                vertexIndex++;
            }
        }

        meshData.BakeNormals();

        return meshData;
    }
}

public class MeshData
{
    Vector3[] vertices;
    int[] triangles;
    Vector2[] UVs;
    Vector3[] bakedNormals;

    Vector3[] borderVertices;
    int[] borderTriangles;

    int borderTriangleIndex;
    int triangleIndex;

    public MeshData(int verticesPerLine)
    {
        vertices = new Vector3[verticesPerLine * verticesPerLine];
        UVs = new Vector2[verticesPerLine * verticesPerLine];
        triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];
        
        borderVertices = new Vector3[verticesPerLine * 4 + 4];
        borderTriangles = new int[24 * verticesPerLine];
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 vertexUV, int vertexIndex)
    {
        if(vertexIndex < 0)
        {
            borderVertices[-vertexIndex - 1] = vertexPosition;
        }
        else
        {
            vertices[vertexIndex] = vertexPosition;
            UVs[vertexIndex] = vertexUV;
        }
    }

    public void AddTriangle(int a, int b, int c)
    {
        if (a < 0 || b < 0 || c < 0)
        {
            borderTriangles[borderTriangleIndex] = a;
            borderTriangles[borderTriangleIndex + 1] = b;
            borderTriangles[borderTriangleIndex + 2] = c;

            borderTriangleIndex += 3;
        }
        else
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;

            triangleIndex += 3;
        }
    }

    public void BakeNormals()
    {
        bakedNormals = CalculateNormals();
    }

    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];

        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertA = triangles[normalTriangleIndex];
            int vertB = triangles[normalTriangleIndex + 1];
            int vertC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertA, vertB, vertC);
            vertexNormals[vertA] += triangleNormal;
            vertexNormals[vertB] += triangleNormal;
            vertexNormals[vertC] += triangleNormal;
        }

        int borderTriangleCount = borderTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertA = borderTriangles[normalTriangleIndex];
            int vertB = borderTriangles[normalTriangleIndex + 1];
            int vertC = borderTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertA, vertB, vertC);
            if (vertA >= 0)
            {
                vertexNormals[vertA] += triangleNormal;
            }
            if (vertB >= 0)
            {
                vertexNormals[vertB] += triangleNormal;
            }
            if (vertC >= 0)
            {
                vertexNormals[vertC] += triangleNormal;
            }
        }

        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }
    
    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0)? borderVertices[-indexA - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? borderVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? borderVertices[-indexC - 1] : vertices[indexC];

        Vector3 AB = pointB - pointA;
        Vector3 AC = pointC - pointA;

        return Vector3.Cross(AB, AC).normalized;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = UVs;
        mesh.normals = bakedNormals;

        return mesh;
    }
}