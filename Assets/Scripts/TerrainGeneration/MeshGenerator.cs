using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(HeightMap heightMap, int levelOfDetail, MeshSettings settings)
    {

        int skipIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int numVertsPerLine = settings.numVertsPerLine;
        Vector2 topLeft = new Vector2(-1, 1) * settings.meshWorldSize / 2f;

        MeshData meshData = new MeshData(numVertsPerLine, skipIncrement, settings.useFlatShading);
        int[,] vertexIndicesMap = new int[numVertsPerLine, numVertsPerLine];
        int meshVertexIndex = 0;
        int outOfMeshVertexIndex = -1;
        for (int y = 0; y < numVertsPerLine; y++)
        {
            for (int x = 0; x < numVertsPerLine; x++)
            {
                bool isOutOfMeshVertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1;
                bool isSkippedVertex = (x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 &&
                                        (((x - 2) % skipIncrement != 0) || ((y - 2) % skipIncrement != 0)));
                if (isOutOfMeshVertex)
                {
                    vertexIndicesMap[x, y] = outOfMeshVertexIndex;
                    outOfMeshVertexIndex--;
                }
                else if (!isSkippedVertex)
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }
        for (int y = 0; y < numVertsPerLine; y++)
        {
            for (int x = 0; x < numVertsPerLine; x++)
            {
                bool isOutOfMeshVertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1;
                bool isSkippedVertex = (x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 &&
                                        (((x - 2) % skipIncrement != 0) || ((y - 2) % skipIncrement != 0)));
                bool isMeshEdgeVertex = (y == 1 || y == numVertsPerLine - 2 || x == 1 || x == numVertsPerLine - 2) && !isOutOfMeshVertex;
                bool isMainVertex = ((x - 2) % skipIncrement == 0) && ((y - 2) % skipIncrement == 0) && !isOutOfMeshVertex && !isMeshEdgeVertex;
                bool isEdgeCxnVertex = (y == 2 || y == numVertsPerLine - 3 || x == 2 || x == numVertsPerLine - 3) && !isOutOfMeshVertex && !isMeshEdgeVertex && !isMainVertex;
                
                if (!isSkippedVertex)
                {
                    int vertexIndex = vertexIndicesMap[x, y];
                    Vector2 percent = new Vector2(x - 1, y - 1) / (numVertsPerLine - 3);
                    Vector2 vertPosition2D = topLeft + new Vector2(percent.x, -percent.y) * settings.meshWorldSize;
                    float height = heightMap.values[x, y] * heightMap.heightMultiplier;

                    if (isEdgeCxnVertex)
                    {
                        bool isVertical = (x == 2) || (x == numVertsPerLine - 3);
                        int dstToMainVertA = (isVertical ? y - 2 : x - 2) % skipIncrement;
                        int dstToMainVertB = skipIncrement - dstToMainVertA;
                        float heightMainVertA = heightMap.values[isVertical ? x : x - dstToMainVertA, isVertical ? y - dstToMainVertA : y] * heightMap.heightMultiplier;
                        float heightMainVertB = heightMap.values[isVertical ? x : x + dstToMainVertB, isVertical ? y + dstToMainVertB : y] * heightMap.heightMultiplier;
                        float dstPercentAToB = dstToMainVertA / (float)skipIncrement;
                        height = heightMainVertA * (1 - dstPercentAToB) + heightMainVertB * dstPercentAToB;
                    }
                    meshData.AddVertex(new Vector3(vertPosition2D.x, height, vertPosition2D.y), percent, vertexIndex);
                    meshData.heightMinMaxValues.AddValue(height);

                    bool createTriangle = x < numVertsPerLine - 1 && y < numVertsPerLine - 1 && (!isEdgeCxnVertex || (x != 2 && y != 2));
                    if (createTriangle)
                    {
                        int currentIncrement = (isMainVertex && x != numVertsPerLine - 3 && y != numVertsPerLine - 3) ? skipIncrement : 1;
                        int a = vertexIndicesMap[x, y];
                        int b = vertexIndicesMap[x + currentIncrement, y];
                        int c = vertexIndicesMap[x, y + currentIncrement];
                        int d = vertexIndicesMap[x + currentIncrement, y + currentIncrement];
                        meshData.AddTriangle(a, d, c);
                        meshData.AddTriangle(d, a, b);
                    }
                }
            }
        }
        meshData.FinalizeMesh();
        return meshData;
    }

    //private static void FindSpaceForTrees(ref MeshData meshData)
    //{
    //    var mesh = meshData.CreateMesh();
    //    //find space for trees
    //    var points = PDSampling.GeneratePoints(1.7f, new Vector2(mesh.bounds.size.x, mesh.bounds.size.z));
    //    foreach (var point in points)
    //    {
    //        float pointElevation = shapeGen.GetScaledElevation(shapeGen.CalculateUnscaledElevation(new Vector3(point.x, 0f, point.y)));

    //        if (pointElevation <= (0.7f * shapeGen.elevationMinMax.Max))
    //        {
    //            if (pointElevation >= (0.2f * shapeGen.elevationMinMax.Max))
    //            {
    //                meshData.AddFoliageLocation(new Vector3(point.x, pointElevation, point.y));
    //            }
    //        }
    //    }
    //}
}

public class MeshData
{
    Vector3[] vertices;
    int[] triangles;
    Vector2[] UVs;
    Vector3[] bakedNormals;

    Vector3[] outOfMeshVerts;
    int[] outOfMeshTriangles;

    int outOfMeshTriangleIndex;
    int triangleIndex;

    bool useFlatShading;

    List<Vector3> foliageLocations;

    public MinMax heightMinMaxValues = new MinMax();

    public MeshData(int verticesPerLine, int skipIncrement, bool useFlatShade)
    {
        
        int numEdgeVertices = (verticesPerLine - 2) * 4 - 4;
        int numEdgeCxnVertices = (skipIncrement - 1) * (verticesPerLine - 5) / skipIncrement * 4;
        int numMainVerticesPerLine = (verticesPerLine - 5) / skipIncrement + 1;
        int numMainVertices = numMainVerticesPerLine * numMainVerticesPerLine;

        vertices = new Vector3[numEdgeVertices + numEdgeCxnVertices + numMainVertices];
        UVs = new Vector2[vertices.Length];

        int numMeshEdgeTriangles = 8 * (verticesPerLine - 4);
        int numMainTriangles = (numMainVerticesPerLine - 1) * (numMainVerticesPerLine - 1) * 2;
        triangles = new int[(numMeshEdgeTriangles + numMainTriangles) * 3];

        outOfMeshVerts = new Vector3[verticesPerLine * 4 - 4];
        outOfMeshTriangles = new int[24 * (verticesPerLine - 2)];

        useFlatShading = useFlatShade;

        foliageLocations = new List<Vector3>();
    }

    public Vector3[] GetFoliagePoints()
    {
        return foliageLocations.ToArray();
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 vertexUV, int vertexIndex)
    {
        if(vertexIndex < 0)
        {
            outOfMeshVerts[-vertexIndex - 1] = vertexPosition;
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
            outOfMeshTriangles[outOfMeshTriangleIndex] = a;
            outOfMeshTriangles[outOfMeshTriangleIndex + 1] = b;
            outOfMeshTriangles[outOfMeshTriangleIndex + 2] = c;

            outOfMeshTriangleIndex += 3;
        }
        else
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;

            triangleIndex += 3;
        }
    }

    void BakeNormals()
    {
        bakedNormals = CalculateNormals();
    }

    public void AddFoliageLocation(Vector3 location)
    {
        foliageLocations.Add(location);
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

        int borderTriangleCount = outOfMeshTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertA = outOfMeshTriangles[normalTriangleIndex];
            int vertB = outOfMeshTriangles[normalTriangleIndex + 1];
            int vertC = outOfMeshTriangles[normalTriangleIndex + 2];

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
        Vector3 pointA = (indexA < 0)? outOfMeshVerts[-indexA - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? outOfMeshVerts[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? outOfMeshVerts[-indexC - 1] : vertices[indexC];

        Vector3 AB = pointB - pointA;
        Vector3 AC = pointC - pointA;

        return Vector3.Cross(AB, AC).normalized;
    }

    void FlatShading()
    {
        Vector3[] flatShadeVerts = new Vector3[triangles.Length];
        Vector2[] flatShadeUVs = new Vector2[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            flatShadeVerts[i] = vertices[triangles[i]];
            flatShadeUVs[i] = UVs[triangles[i]];
            triangles[i] = i;
        }

        vertices = flatShadeVerts;
        UVs = flatShadeUVs;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = UVs;

        if (useFlatShading)
        { 
            mesh.RecalculateNormals();
        }
        else
        {
            mesh.normals = bakedNormals;
        }
        return mesh;
    }

    public void FinalizeMesh()
    {
        if(useFlatShading)
        {
            FlatShading();
        }
        else
        {
            BakeNormals();
        }
    }
}