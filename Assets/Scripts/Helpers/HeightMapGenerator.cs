using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightMapGenerator
{
    static float[,] falloffMapData;

    static float[,] CreateNoiseMap(int mapWidth, int mapHeight, HeightMapSettings settings, Vector2 sampleCentre)
    {
        float[,] toReturn = new float[mapWidth, mapHeight];
        falloffMapData = FalloffGenerator.GenerateFalloffMap(mapWidth);

        INoiseFilter[] noiseFilters = new INoiseFilter[settings.noiseLayerSettings.noiseLayers.Length];
        for (int i = 0; i < noiseFilters.Length; i++)
        {
            noiseFilters[i] = NoiseFilterFactory.CreateNoiseFilter(settings.noiseLayerSettings.noiseLayers[i].noiseSettings);
        }

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                float firstLayerValue = 0;
                float elevation = 0;

                Vector3 currentCoordinate = new Vector3(sampleCentre.x, 0f, sampleCentre.y);

                currentCoordinate.x += i + (mapWidth / 2);
                currentCoordinate.z -= j + (mapHeight / 2);

                if (noiseFilters.Length > 0)
                {
                    firstLayerValue = noiseFilters[0].Evaluate(currentCoordinate);
                    if (settings.noiseLayerSettings.noiseLayers[0].enabled)
                    {
                        elevation = firstLayerValue;
                    }
                }

                for (int k = 1; k < noiseFilters.Length; k++)
                {
                    if (settings.noiseLayerSettings.noiseLayers[k].enabled)
                    {
                        float mask = (settings.noiseLayerSettings.noiseLayers[k].useFirstLayerAsMask ? firstLayerValue : 1);
                        elevation += noiseFilters[k].Evaluate(currentCoordinate) * mask;
                    }
                }

                if(settings.useFalloffMap)
                {
                    elevation *= 1 - falloffMapData[i, j];
                }    


                toReturn[i, j] = elevation;
            }
        }

        return toReturn;
    }

    public static HeightMap GetHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCentre)
    {
        var values = CreateNoiseMap(width, height, settings, sampleCentre);

        MinMax minMaxValues = new MinMax();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                minMaxValues.AddValue(values[i, j]);
            }
        }

        return new HeightMap(values, minMaxValues.Min, minMaxValues.Max, settings.heightMultiplier);
    }
}

public struct HeightMap
{
    public readonly float[,] values;
    public readonly float minValue;
    public readonly float maxValue;
    
    public readonly MinMax minMaxValues;

    public float heightMultiplier;

    public HeightMap(float[,] heightMap, float min, float max, float mult)
    {
        values = heightMap;
        minValue = min;
        maxValue = max;
        minMaxValues = new MinMax();
        minMaxValues.AddValue(min);
        minMaxValues.AddValue(max);
        heightMultiplier = mult;
    }
}