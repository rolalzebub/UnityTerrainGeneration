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

    public Material terrainMaterial;

    [HideInInspector]
    public bool meshSettingsFoldout;
    [HideInInspector]
    public bool textureDataFoldout;
    [HideInInspector]
    public bool shapeSettingsFoldout;
    [HideInInspector]
    public bool heightSettingsFoldout;

    public GameObject[] grassyObjectsToPlace;
    public float grassyObjectPlacementRadius = 3f;

    List<GameObject> placedTrees = new List<GameObject>();
    public FoliageSettings foliageSettings;

    HeightMap previewHeightmap;
    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;

        textureRenderer.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }
    
    public void DrawMesh(MeshData meshData)
    {
        textureData.UpdateMeshHeights(terrainMaterial, meshData.heightMinMaxValues.Min, meshData.heightMinMaxValues.Max);
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
            

            Ray pointCheck = new Ray(new Vector3(meshFilter.sharedMesh.bounds.min.x + point.x, meshFilter.sharedMesh.bounds.max.y, meshFilter.sharedMesh.bounds.min.z + point.y), Vector3.down);
            RaycastHit checkInfo;
            bool checkResult = Physics.Raycast(pointCheck, out checkInfo, meshFilter.sharedMesh.bounds.size.y * 1.5f);

            if (checkResult)
            {
                if (checkInfo.collider.attachedRigidbody == null)
                {
                    Vector2 percent = new Vector2( point.x / meshFilter.sharedMesh.bounds.size.x, point.y / meshFilter.sharedMesh.bounds.size.z);
                    
                    Vector2Int elevationPoint = new Vector2Int(Mathf.FloorToInt(percent.x * meshSettings.numVertsPerLine), Mathf.FloorToInt(percent.y * meshSettings.numVertsPerLine)); ;

                    float pointElevation = previewHeightmap.values[elevationPoint.x, elevationPoint.y];

                    var toSpawn = FoliageFactory.GetFoliageForPoint(new FoliagePointProfile() { pointElevation = pointElevation, elevationMinMax = previewHeightmap.minMaxValues });
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
        FoliageFactory.SetFoliageSettings(foliageSettings);
        textureData.ApplyToMaterial(terrainMaterial);
        if(heightSettings.useFalloffMap)
            FalloffGenerator.SetCurve(heightSettings.falloffMapCurve);

        previewHeightmap = HeightMapGenerator.GetHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightSettings, Vector2.zero);

        if (drawMode == DrawMode.NoiseMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightMap(previewHeightmap));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            DrawMesh(MeshGenerator.GenerateTerrainMesh(previewHeightmap, editorPreviewLOD, meshSettings));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numVertsPerLine), 0, 1, 1f)));
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
