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

        int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

        float topLeftX = (meshSizeUnsimplified - 1) / -2f;
        float topLeftZ = (meshSizeUnsimplified - 1) / 2f;

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
                Vector3 vertexPosition = new Vector3 (topLeftX + percent.x * meshSizeUnsimplified, height, topLeftZ - percent.y * meshSizeUnsimplified);

                meshData.AddVertex(vertexPosition, percent, vertexIndex);

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
        return meshData;
    }

}

public class MeshData
{
    Vector3[] vertices;
    int[] triangles;
    Vector2[] UVs;

    Vector3[] borderVertices;
    int[] borderTriangles;

    int triangleIndex;
    int borderTriangleIndex;

    public MeshData(int verticesPerLine)
    {
        vertices = new Vector3[verticesPerLine * verticesPerLine];
        UVs = new Vector2[verticesPerLine * verticesPerLine];
        triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];

        borderVertices = new Vector3[verticesPerLine * 4 + 4];
        borderTriangles = new int[24 * verticesPerLine];
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        if(vertexIndex<0)
        {
            borderVertices[-vertexIndex - 1] = vertexPosition;
        }
        else
        {
            vertices[vertexIndex] = vertexPosition;
            UVs[vertexIndex] = uv;
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

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = UVs;
        mesh.normals = CalculateNormals();

        return mesh;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = indexA < 0 ? borderVertices[-indexA-1] : vertices[indexA];
        Vector3 pointB = indexB < 0 ? borderVertices[-indexB-1] : vertices[indexB];
        Vector3 pointC = indexC < 0 ? borderVertices[-indexC-1] : vertices[indexC];

        Vector3 AB = pointB - pointA;
        Vector3 AC = pointC - pointA;

        return Vector3.Cross(AB, AC).normalized;
    }

    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triCount = triangles.Length / 3;
        for (int i = 0; i < triCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertIndexA = triangles[normalTriangleIndex];
            int vertIndexB = triangles[normalTriangleIndex + 1];
            int vertIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertIndexA, vertIndexB, vertIndexC);
            vertexNormals[vertIndexA] += triangleNormal;
            vertexNormals[vertIndexB] += triangleNormal;
            vertexNormals[vertIndexC] += triangleNormal;
        }

        int borderTriCount = borderTriangles.Length / 3;
        for (int i = 0; i < borderTriCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertIndexA = borderTriangles[normalTriangleIndex];
            int vertIndexB = borderTriangles[normalTriangleIndex + 1];
            int vertIndexC = borderTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertIndexA, vertIndexB, vertIndexC);
            if (vertIndexA >= 0)
            {
                vertexNormals[vertIndexA] += triangleNormal;
            }
            if (vertIndexB >= 0)
            {
                vertexNormals[vertIndexB] += triangleNormal;
            }
            if (vertIndexC >= 0)
            {
                vertexNormals[vertIndexC] += triangleNormal;
            }
        }

        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }
}