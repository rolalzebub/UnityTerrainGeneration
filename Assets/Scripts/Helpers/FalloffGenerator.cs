using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FalloffGenerator
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

            else
            {
                float[,] map = new float[size, size];
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        float x = i / (float)size * 2 - 1;
                        float y = j / (float)size * 2 - 1;

                        float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                        map[i, j] = Evaluate(value);
                    }
                }

                cachedMap = map;
                cachedSize = size;
                isCached = true;
            }
            return cachedMap;
        }
    }

    static float Evaluate(float value)
    {
        AnimationCurve threadSafeCurve = falloffCurve;

        if (falloffCurve != null)
        {
            return threadSafeCurve.Evaluate(value);
        }

        //default method
        //by sebastian lague
        float a = 3;
        float b = 2.2f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}
