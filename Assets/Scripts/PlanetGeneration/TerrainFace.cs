using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace
{
    Mesh mesh;
    int vertsPerLine;
    Vector3 localUp;
    Vector3 axisA, axisB;
    ShapeGenerator shapeGenerator;

    public TerrainFace(Mesh mesh, int resolution, Vector3 localUp, ShapeGenerator _shapeGen)
    {
        this.mesh = mesh;
        this.vertsPerLine = resolution;
        this.localUp = localUp;
        shapeGenerator = _shapeGen;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public void ConstructMesh()
    {
        Vector3[] vertices = new Vector3[vertsPerLine * vertsPerLine];
        int[] triangles = new int[(vertsPerLine - 1) * (vertsPerLine - 1) * 6];

        int triIndex = 0;

        for (int y = 0; y < vertsPerLine; y++)
        {
            for (int x = 0; x < vertsPerLine; x++)
            {
                int i = x + y * vertsPerLine;
                Vector2 percent = new Vector2(x, y) / (vertsPerLine - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                vertices[i] = shapeGenerator.CalculatePointOnPlanet(pointOnUnitSphere);

                if(x != vertsPerLine - 1 && y != vertsPerLine - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + vertsPerLine + 1;
                    triangles[triIndex + 2] = i + vertsPerLine;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + vertsPerLine + 1;

                    triIndex += 6;
                }
            }
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

}
