using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
	public event System.Action<TerrainChunk, bool> OnVisibilityChanged;

	const float colliderGenerationDstThreshold = 5f;
	
	public Vector2 coord;

	GameObject meshObject;
	Vector2 sampleCentre;
	Bounds bounds;

	MeshRenderer meshRenderer;
	MeshFilter meshFilter;
	MeshCollider meshCollider;

	LODInfo[] detailLevels;
	LODMesh[] lodMeshes;
	int colliderLODIndex;

	HeightMap heightMap;
	bool heightMapReceived;
	int previousLODIndex = -1;
	bool hasSetCollider = false;

	float maxViewDistance;

	Transform viewer;

	HeightMapSettings heightSettings;
	MeshSettings meshSettings;

	Vector2 viewerPosition
    {
		get
		{ return new Vector2(viewer.position.x, viewer.position.z); }
    }
	public TerrainChunk(Vector2 _coord, LODInfo[] detailLevels, int _colliderLODIndex,
		Transform parent, Material material, HeightMapSettings heightMapSettings, MeshSettings _meshSettings, Transform _viewer)
	{
		this.detailLevels = detailLevels;
		this.colliderLODIndex = _colliderLODIndex;
		this.coord = _coord;
		heightSettings = heightMapSettings;
		meshSettings = _meshSettings;
		viewer = _viewer;



		sampleCentre = _coord * meshSettings.meshWorldSize / meshSettings.meshScale;
		Vector2 position = coord * meshSettings.meshWorldSize;
		bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

		meshObject = new GameObject("Terrain Chunk");
		meshRenderer = meshObject.AddComponent<MeshRenderer>();
		meshFilter = meshObject.AddComponent<MeshFilter>();
		meshCollider = meshObject.AddComponent<MeshCollider>();
		meshRenderer.material = material;

		meshObject.transform.position = new Vector3(position.x, 0, position.y);
		meshObject.transform.parent = parent;
		SetVisible(false);

		lodMeshes = new LODMesh[detailLevels.Length];
		for (int i = 0; i < detailLevels.Length; i++)
		{
			lodMeshes[i] = new LODMesh(detailLevels[i].lod, coord);
			lodMeshes[i].updateCallback += UpdateTerrainChunk;
			if (i == colliderLODIndex)
			{
				lodMeshes[i].updateCallback += UpdateCollisionMesh;
			}
		}

		maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
	}

	void OnHeightMapReceived(object mapData)
	{
		this.heightMap = (HeightMap)mapData;
		heightMapReceived = true;

		UpdateTerrainChunk();
	}

	public void Load()
    {
		ThreadedDataRequestor.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightSettings, sampleCentre),
			OnHeightMapReceived);
	}

	public void UpdateTerrainChunk()
	{
		if (heightMapReceived)
		{
			float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
			bool wasVisible = IsVisible();
			bool visible = viewerDstFromNearestEdge <= maxViewDistance;

			if (visible)
			{
				int lodIndex = 0;

				for (int i = 0; i < detailLevels.Length - 1; i++)
				{
					if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
					{
						lodIndex = i + 1;
					}
					else
					{
						break;
					}
				}

				if (lodIndex != previousLODIndex)
				{
					LODMesh lodMesh = lodMeshes[lodIndex];
					if (lodMesh.hasMesh)
					{
						previousLODIndex = lodIndex;
						meshFilter.mesh = lodMesh.mesh;
					}
					else if (!lodMesh.hasRequestedMesh)
					{
						lodMesh.RequestMesh(heightMap, meshSettings);
					}
				}
			}

			if (wasVisible != visible)
			{
				SetVisible(visible);
				if (OnVisibilityChanged != null)
                {
					OnVisibilityChanged(this, visible);
                }
			}
		}
	}

	public void UpdateCollisionMesh()
	{
		if (hasSetCollider)
		{
			return;
		}

		float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

		if (sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThreshold)
		{
			if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
			{
				lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
			}
		}


		if (sqrDstFromViewerToEdge < colliderGenerationDstThreshold * colliderGenerationDstThreshold)
		{
			if (lodMeshes[colliderLODIndex].hasMesh)
			{
				meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
				hasSetCollider = true;
			}
		}
	}

	public void SetVisible(bool visible)
	{
		meshObject.SetActive(visible);
	}

	public bool IsVisible()
	{
		return meshObject.activeSelf;
	}

}

class LODMesh
{

	public Mesh mesh;
	public bool hasRequestedMesh;
	public bool hasMesh;
	Vector2 chunkCoord;
	int lod;
	public event System.Action updateCallback;

	public LODMesh(int lod, Vector2 _chunkCoord)
	{
		this.lod = lod;
		chunkCoord = _chunkCoord;
	}

	void OnMeshDataReceived(object meshData)
	{
		mesh = ((MeshData)meshData).CreateMesh();
		hasMesh = true;

		updateCallback();
	}

	public void RequestMesh(HeightMap heightMap, MeshSettings settings)
	{
		hasRequestedMesh = true;
		ThreadedDataRequestor.RequestData(() => MeshGenerator.GenerateTerrainMesh(lod, settings, chunkCoord), OnMeshDataReceived);
	}

}
