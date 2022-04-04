using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
	public enum DrawMode { NoiseMap, Mesh, FalloffMap };
	public DrawMode drawMode;

	[Range(0, MeshSettings.numSupportedLODs - 1)]
	public int editorPreviewLOD;
	
	public bool autoUpdate;
	
	public HeightMapSettings heightMapSettings;
	public MeshSettings meshSettings;
	public TextureData textureData;

	public Material terrainMaterial;

	Queue<MapThreadInfo<HeightMap>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<HeightMap>>();
	Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    private void Start()
    {
		textureData.ApplyToMaterial(terrainMaterial);
		textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
	}

    public void DrawMapInEditor()
	{

		HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, Vector2.zero);
		textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

		MapDisplay display = FindObjectOfType<MapDisplay>();
		if (drawMode == DrawMode.NoiseMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap.values));
		}
		else if (drawMode == DrawMode.Mesh)
		{
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, editorPreviewLOD, meshSettings));
		}
		else if(drawMode == DrawMode.FalloffMap)
        {
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numVertsPerLine)));
        }
	}

	public void RequestHeightMap(Vector2 centre, Action<HeightMap> callback)
	{
		ThreadStart threadStart = delegate {
			HeightMapThread(centre, callback);
		};

		new Thread(threadStart).Start();
	}

	void HeightMapThread(Vector2 centre, Action<HeightMap> callback)
	{
		HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, centre);
		lock (mapDataThreadInfoQueue)
		{
			mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<HeightMap>(callback, heightMap));
		}
	}

	public void RequestMeshData(HeightMap mapData, int lod, Action<MeshData> callback)
	{
		ThreadStart threadStart = delegate {
			MeshDataThread(mapData, lod, callback);
		};

		new Thread(threadStart).Start();
	}

	void MeshDataThread(HeightMap heightMap, int lod, Action<MeshData> callback)
	{
		MeshData meshData = MeshGenerator.GenerateTerrainMesh(heightMap.values, lod, meshSettings);
		lock (meshDataThreadInfoQueue)
		{
			meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
		}
	}

	void Update()
	{
		if (mapDataThreadInfoQueue.Count > 0)
		{
			for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
			{
				MapThreadInfo<HeightMap> threadInfo = mapDataThreadInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
		}

		if (meshDataThreadInfoQueue.Count > 0)
		{
			for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
			{
				MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
		}
	}

	//HeightMap GenerateMapData(Vector2 centre)
	//{
	//	float[,] noiseMap = Noise.GenerateNoiseMap(noiseData.seed, terrainData.numVertsPerLine + 2, terrainData.numVertsPerLine + 2, noiseData.noiseScale, noiseData.octaves, 
	//		noiseData.persistance, noiseData.lacunarity, centre + noiseData.offset, noiseData.normalizeMode);

	//	if (noiseData.useFalloffMap)
	//	{
	//		if (noiseData.falloffMap == null)
	//		{
	//			noiseData.CalculateFalloffMap(terrainData.numVertsPerLine);
	//		}

	//		for (int y = 0; y < terrainData.numVertsPerLine + 2; y++)
	//		{
	//			for (int x = 0; x < terrainData.numVertsPerLine + 2; x++)
	//			{
	//				if (noiseData.useFalloffMap)
	//				{
	//					noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - noiseData.falloffMap[x, y]);
	//				}
	//			}
	//		}
	//	}

	//	return new HeightMap(noiseMap);
	//}

	void OnValidate()
	{
		if (meshSettings != null)
		{
			meshSettings.OnValuesUpdated -= OnValuesUpdated;
			meshSettings.OnValuesUpdated += OnValuesUpdated;
		}

		if(heightMapSettings != null)
        {
			heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
			heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }

		if(textureData!= null)
        {
			textureData.OnValuesUpdated -= OnTextureValuesUpdated;
			textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
	}

	void OnTextureValuesUpdated()
    {
		textureData.ApplyToMaterial(terrainMaterial);
    }

	void OnValuesUpdated()
    {
		if(!Application.isPlaying)
        {
			DrawMapInEditor();
        }
    }

	struct MapThreadInfo<T>
	{
		public readonly Action<T> callback;
		public readonly T parameter;

		public MapThreadInfo(Action<T> callback, T parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}
	}
}