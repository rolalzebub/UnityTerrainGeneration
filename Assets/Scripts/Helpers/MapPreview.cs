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

    public MeshSettings meshSettings;
    public HeightMapSettings heightSettings;
    public TextureData textureData;
    public ShapeSettings shapeSettings;

    ShapeGenerator shapeGen = new ShapeGenerator();

    public Material terrainMaterial;

    [HideInInspector]
    public bool meshSettingsFoldout;
    [HideInInspector]
    public bool textureDataFoldout;
    [HideInInspector]
    public bool shapeSettingsFoldout;

    public GameObject[] grassyObjectsToPlace;
    public float grassyObjectPlacementRadius = 3f;

    List<GameObject> placedTrees = new List<GameObject>();
    public FoliageSettings foliageSettings;
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

    void ClearOldTrees()
    {
        if(placedTrees.Count > 0)
        {
            foreach(var tree in placedTrees)
            {
                DestroyImmediate(tree);
            }
            placedTrees.Clear();
        }
    }

    private void FindSpaceForTrees()
    {
        ClearOldTrees();

        //find space for trees
        var points = PDSampling.GeneratePoints(grassyObjectPlacementRadius, new Vector2(meshFilter.sharedMesh.bounds.size.x, meshFilter.sharedMesh.bounds.size.z));
        foreach (var point in points)
        {
            //raycast upwards from point till you hit a triangle
            Ray pointCheck = new Ray(new Vector3(meshFilter.sharedMesh.bounds.min.x + point.x, 100, meshFilter.sharedMesh.bounds.min.z + point.y), Vector3.down);
            RaycastHit checkInfo;
            bool checkResult = Physics.Raycast(pointCheck, out checkInfo, 120f);

            if (checkResult)
            {
                if (checkInfo.collider.attachedRigidbody == null)
                {
                    float pointElevation = shapeGen.CalculateUnscaledElevation(new Vector3(checkInfo.point.x, checkInfo.point.y, checkInfo.point.z));

                    var toSpawn = FoliageFactory.GetFoliageForPoint(new FoliagePointProfile() { pointElevation = pointElevation, elevationMinMax = shapeGen.elevationMinMax });
                    if (toSpawn != null)
                    {
                        var tree = Instantiate(toSpawn, checkInfo.point, Quaternion.identity, meshFilter.gameObject.transform);
                        tree.transform.up = checkInfo.normal;
                        placedTrees.Add(tree);
                    }
                }
            }
        }
    }

    public void DrawMapInEditor()
    {
        shapeGen.UpdateSettings(shapeSettings);
        FoliageFactory.SetFoliageSettings(foliageSettings);
        MeshGenerator.UpdateShapeGenerator(shapeGen);
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightSettings.minHeight, heightSettings.maxHeight);

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

        if (heightSettings != null)
        {
            heightSettings.OnValuesUpdated -= OnValuesUpdated;
            heightSettings.OnValuesUpdated += OnValuesUpdated;
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
