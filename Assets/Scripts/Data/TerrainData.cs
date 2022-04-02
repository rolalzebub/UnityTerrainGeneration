using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : ScriptableObject
{
	public float uniformScale = 1f;

	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;

	public bool useFlatShading;

	public bool useFalloffMap;
	public AnimationCurve falloffMapCurve;

    private void OnValidate()
    {
        if(useFalloffMap)
        {
            FalloffGenerator.SetCurve(falloffMapCurve);
        }
    }
}
