using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseLayerSettings : ScriptableObject
{
    public NoiseLayer[] noiseLayers;

    [System.Serializable]
    public class NoiseLayer
    {
        public bool enabled = true;
        public bool useFirstLayerAsMask = true;
        public SimplexNoiseSettings noiseSettings;
    }

    public void ValidateValues()
    {
        foreach(var layer in noiseLayers)
        {
            layer.noiseSettings.simpleNoiseSettings.octaves = Mathf.Max(layer.noiseSettings.simpleNoiseSettings.octaves, 1);
            layer.noiseSettings.simpleNoiseSettings.lacuranity = Mathf.Max(layer.noiseSettings.simpleNoiseSettings.lacuranity, 1);
            layer.noiseSettings.simpleNoiseSettings.persistence = Mathf.Clamp01(layer.noiseSettings.simpleNoiseSettings.persistence);
        }
    }
}
