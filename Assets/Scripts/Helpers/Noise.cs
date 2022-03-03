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
    public static float[,] GenerateNoiseMap(int seed, int mapWidth, int mapHeight, float scale, 
        int octaves, float persistence, float lacunarity,
        Vector2 offset, NormalizeMode normMode)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }

        //clamp scale to a minimum value to avoid div by zero errors
        if (scale <= 0)
        {
            scale = 0.0001f;
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

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight) 
                {
                    maxLocalNoiseHeight = noiseHeight;
                } 
                else if (noiseHeight < minLocalNoiseHeight) 
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                 noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (normMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / maxPossibleHeight;
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        return noiseMap;
    }
}
