using UnityEngine;

public static class Noise
{

    public enum NormalizeMode { Local, Global };

    /// <summary>
    /// Method to generate noise map
    /// </summary>
    /// <param name="seed">Map generation seed</param>
    /// <param name="mapWidth"></param>
    /// <param name="mapHeight"></param>
    /// <param name="scale"></param>
    /// <param name="octaves">Used to determine how many layers of detail should be generated</param>
    /// <param name="persistence">Scale of effect each subsequent octave after the first</param>
    /// <param name="lacunarity">Number of layers for detail/small features (scale factor for frequency of subsuquent octaves)</param>
    /// <param name="offset">Offset to add to samples while sampling the noise map</param>
    /// <returns></returns>
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCentre)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(settings.seed);
        Vector2[] octaveOffsets = new Vector2[settings.octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;

        for (int i = 0; i < settings.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCentre.x;
            float offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCentre.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= settings.persistence;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        //produce a noise map based on scaled map height and map width
        for(int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                //reset amplitude and frequency
                amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < settings.octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / settings.scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    noiseHeight += perlinValue * amplitude;

                    amplitude *= settings.persistence;
                    frequency *= settings.lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight) 
                {
                    maxLocalNoiseHeight = noiseHeight;
                } 
                if (noiseHeight < minLocalNoiseHeight) 
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                 noiseMap[x, y] = noiseHeight;

                if (settings.normalizeMode == NormalizeMode.Global)
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / maxPossibleHeight;
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        if (settings.normalizeMode == NormalizeMode.Local)
        {

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
            }
        }
        return noiseMap;
    }
}

[System.Serializable]
public class NoiseSettings
{
    public Noise.NormalizeMode normalizeMode;

    public float scale = 50;

    public int octaves = 6;
    [Range(0, 1)]
    public float persistence = 0.6f;
    public float lacunarity = 2f;

    public int seed;
    public Vector2 offset;

    public void ValidateValues()
    {
        scale = Mathf.Max(scale, 0.01f);
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistence = Mathf.Clamp01(persistence);
    }
}