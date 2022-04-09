using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseFilter
{
    SimplexNoise noise = new SimplexNoise();
    SimplexNoiseSettings settings;

    public NoiseFilter(SimplexNoiseSettings _settings)
    {
        settings = _settings;
    }

    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float frequency = settings.baseRoughness;
        float amplitude = 1;

        for (int i = 0; i < settings.octaves; i++)
        {
            float v = noise.Evaluate(point * frequency + settings.centre);
            noiseValue += (v + 1) * .5f * amplitude;
            frequency = frequency * settings.roughness;
            amplitude = amplitude * settings.persistence;
        }

        noiseValue = Mathf.Max(0, noiseValue - settings.minValue);
        return noiseValue * settings.strength;
    }
}