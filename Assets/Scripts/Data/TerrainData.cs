using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : UpdatableData
{
	public float uniformScale = 1f;

	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;

	public bool useFlatShading;

	public bool useFalloffMap;
	public AnimationCurve falloffMapCurve;

}
