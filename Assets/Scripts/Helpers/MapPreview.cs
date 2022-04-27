using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPreview : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public enum DrawMode { NoiseMap, Mesh, FalloffMap };
    public DrawMode drawMode;

    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int editorPreviewLOD;

    public bool autoUpdate;

    public HeightMapSettings heightMapSettings;
    public MeshSettings meshSettings;
    public TextureData textureData;
    public ShapeSettings shapeSettings;

    ShapeGenerator shapeGen = new ShapeGenerator();

    public Material terrainMaterial;

    [HideInInspector]
    public bool heightSettingsFoldout;
    [HideInInspector]
    public bool meshSettingsFoldout;
    [HideInInspector]
    public bool textureDataFoldout;
    [HideInInspector]
    public bool shapeSettingsFoldout;

    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);

        textureRenderer.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }
    
    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();

        textureRenderer.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(true);

        FindSpaceForTrees();
    }

    private void FindSpaceForTrees()
    {
        //find space for trees
        var points = PDSampling.GeneratePoints(1.5f, new Vector2(meshFilter.sharedMesh.bounds.size.x, meshFilter.sharedMesh.bounds.size.z));

        foreach (var point in points)
        {
            //raycast upwards from point till you hit a triangle
            Ray pointCheck = new Ray(new Vector3(meshFilter.sharedMesh.bounds.min.x + point.x, 100, meshFilter.sharedMesh.bounds.min.z + point.y), Vector3.down);
            Debug.DrawLine(pointCheck.origin, pointCheck.origin + (pointCheck.direction * 120f));
            RaycastHit checkInfo;
            bool checkResult = Physics.Raycast(pointCheck, out checkInfo, 120f);

            if (checkResult)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.position = checkInfo.point;
                Vector3 surfaceNormal = checkInfo.normal;
                if (Mathf.Abs(Vector3.Angle(surfaceNormal, Vector3.up)) < 25)
                {
                    Debug.Log("This spot works");
                }
            }

        }
    }

    public void DrawMapInEditor()
    {
        shapeGen.UpdateSettings(shapeSettings);
        MeshGenerator.UpdateShapeGenerator(shapeGen);
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        if (drawMode == DrawMode.NoiseMap)
        {
            DrawTexture(TextureGenerator.TextureFromShapeGenerator(new Vector2Int(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine), ref shapeGen));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            DrawMesh(MeshGenerator.GenerateTerrainMesh(editorPreviewLOD, meshSettings, Vector2.zero));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numVertsPerLine), 0, 1)));
        }
    }


    void OnValidate()
    {
        if (meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
        if (shapeSettings != null)
        {
            shapeGen.UpdateSettings(shapeSettings);
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    public void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }
}
