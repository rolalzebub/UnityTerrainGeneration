using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class SimplexNoiseSettings
{
    public enum FilterType { Simple, Ridged };

    public FilterType filterType;
    
    [ConditionalHide("filterType", 0)]
    public SimpleNoiseSettings simpleNoiseSettings;
    [ConditionalHide("filterType", 1)]
    public RidgedNoiseSettings ridgedNoiseSettings;

    [System.Serializable]
    public class SimpleNoiseSettings
    {
        /// <summary>
        /// Number of noise layers to coalesce for final noisemap generation
        /// </summary>
        [Range(1, 8)]
        public int octaves;
        public float baseRoughness = 1f;
        public float roughness = 2f;
        public float persistence = .5f;
        public float lacuranity = 2f;
        public Vector3 centre;
        public float minValue;
    }

    [System.Serializable]
    public class RidgedNoiseSettings: SimpleNoiseSettings
    {
        public float weightMultiplier = 0.8f;
    }

}
