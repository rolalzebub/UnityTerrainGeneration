using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeGenerator
{
    NoiseLayerSettings settings;
    INoiseFilter[] noiseFilters;
    public MinMax elevationMinMax;

    public void UpdateSettings(NoiseLayerSettings _settings)
    {
        settings = _settings;
        noiseFilters = new INoiseFilter[settings.noiseLayers.Length];
        for (int i = 0; i < noiseFilters.Length; i++)
        {
            noiseFilters[i] = NoiseFilterFactory.CreateNoiseFilter(settings.noiseLayers[i].noiseSettings);
        }

        elevationMinMax = new MinMax();
    }

    public NoiseLayerSettings GetCurrentSettings()
    {
        return settings;
    }

    public float CalculateUnscaledElevation(Vector3 pointOnUnitSphere)
    {
        float firstLayerValue = 0;

        float elevation = 0;

        if(noiseFilters.Length > 0)
        {
            firstLayerValue = noiseFilters[0].Evaluate(pointOnUnitSphere);
            if(settings.noiseLayers[0].enabled)
            {
                elevation = firstLayerValue;
            }
        }

        for (int i = 1; i < noiseFilters.Length; i++)
        {
            if (settings.noiseLayers[i].enabled)
            {
                float mask = (settings.noiseLayers[i].useFirstLayerAsMask ? firstLayerValue : 1);
                elevation += noiseFilters[i].Evaluate(pointOnUnitSphere) * mask;
            }
        }


        elevationMinMax.AddValue(elevation);

        return elevation;
    }

    public float GetScaledElevation(float unscaledElevation)
    {
        float elevation = Mathf.Max(0, unscaledElevation);
        elevation = (1 + elevation);
        return elevation;
    }

    public float[,] CreateNoiseMap(int mapWidth, int mapHeight, NoiseLayerSettings settings, Vector2 sampleCentre)
    {
        float[,] toReturn = new float[mapWidth, mapHeight];

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                float firstLayerValue = 0;
                float elevation = 0;

                Vector3 currentCoordinate = new Vector3(sampleCentre.x, 0f, sampleCentre.y);
                currentCoordinate.x -= mapWidth / 2;
                currentCoordinate.x += i;
                currentCoordinate.z -= mapHeight / 2;
                currentCoordinate.z += j;

                if (noiseFilters.Length > 0)
                {
                    firstLayerValue = noiseFilters[0].Evaluate(currentCoordinate);
                    if (settings.noiseLayers[0].enabled)
                    {
                        elevation = firstLayerValue;
                    }
                }

                for (int k = 1; k < noiseFilters.Length; k++)
                {
                    if (settings.noiseLayers[k].enabled)
                    {
                        float mask = (settings.noiseLayers[k].useFirstLayerAsMask ? firstLayerValue : 1);
                        elevation += noiseFilters[k].Evaluate(currentCoordinate) * mask;
                    }
                }

                toReturn[i,j] = elevation;
            }
        }

        return toReturn;
    }
}
