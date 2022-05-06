using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RidgedNoiseFilter : INoiseFilter
{
    SimplexNoise noise = new SimplexNoise();
    SimplexNoiseSettings.RidgedNoiseSettings settings;

    public RidgedNoiseFilter(SimplexNoiseSettings.RidgedNoiseSettings _settings)
    {
        settings = _settings;
    }

    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float frequency = settings.baseRoughness;
        float amplitude = 1;
        float weight = 1;

        for (int i = 0; i < settings.octaves; i++)
        {
            float v = 1 - Mathf.Abs(noise.Evaluate(point * frequency + settings.centre));
            v = v * v;
            v = v * weight;
            weight = Mathf.Clamp01(v * settings.weightMultiplier);

            noiseValue += v * amplitude;
            frequency = frequency * settings.roughness;
            amplitude = amplitude * settings.persistence;
        }

        noiseValue = noiseValue - settings.minValue;
        return noiseValue;
    }
}
