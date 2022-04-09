using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SimplexNoiseSettings
{
    /// <summary>
    /// Number of noise layers to coalesce for final noisemap generation
    /// </summary>
    [Range(1,8)]
    public int octaves;
    public float strength = 1f;
    public float baseRoughness = 1f;
    public float roughness = 2f;
    public float persistence = .5f;
    public Vector3 centre;
    public float minValue;
}
