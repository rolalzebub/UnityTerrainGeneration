using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class HeightMapSettings : UpdatableData
{
	public NoiseSettings noiseSettings;
	
	public float heightMultiplier;
	public AnimationCurve heightCurve;

	public bool useFalloffMap;
	public AnimationCurve falloffMapCurve;

	float[,] falloffMapData;

	public float[,] falloffMap
	{
		get
		{
			return falloffMapData;
		}
	}

	public float minHeight
	{
		get
		{
			return heightMultiplier * heightCurve.Evaluate(0);
		}
	}

	public float maxHeight
	{
		get
		{
			return heightMultiplier * heightCurve.Evaluate(1);
		}
	}

	public void SetFalloffData(float[,] data)
	{
		falloffMapData = data;
	}

	public void CalculateFalloffMap(int chunkSize)
	{
		falloffMapData = FalloffGenerator.GenerateFalloffMap(chunkSize + 2);
	}

#if UNITY_EDITOR
	protected override void OnValidate()
    {
		base.OnValidate();

		noiseSettings.ValidateValues();

		//FalloffGenerator.SetCurve(falloffMapCurve);
		//falloffMapData = FalloffGenerator.GenerateFalloffMap(mapChunkSize + 2);
	}
#endif
}
