using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator
{
    static bool isCached = false;
    static int cachedSize = 0;
    static float[,] cachedMap = new float[0,0];

    static AnimationCurve falloffCurve;

    public static void SetCurve(AnimationCurve curve)
    {
        falloffCurve = curve;
    }

    public static float[,] GenerateFalloffMap(int size)
    {
        lock(cachedMap)
            {
            if (isCached)
            {
                if (cachedSize == size)
                {
                    return cachedMap;
                }
            }

            float[,] map = new float[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    float x = i / (float)size * 2 - 1;
                    float y = j / (float)size * 2 - 1;

                    float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                    map[i, j] = 1 - Evaluate(value);
                }
            }

            cachedMap = map;
            cachedSize = size;
            isCached = true;

            return cachedMap;
        }
    }

    static float Evaluate(float value)
    {
        return falloffCurve.Evaluate(value);
    }
}
