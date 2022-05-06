using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class HeightMapSettings : UpdatableData
{
	public NoiseLayerSettings noiseLayerSettings;

	public float heightMultiplier = 1f;
	public AnimationCurve heightCurve;

	public bool useFalloffMap;
	public AnimationCurve falloffMapCurve;

#if UNITY_EDITOR
	protected override void OnValidate()
    {
		base.OnValidate();

		noiseLayerSettings.ValidateValues();

	}
#endif
}
