using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColourMap(Color[] colourMap, int mapWidth, int mapHeight)
    {
        Texture2D texture = new Texture2D(mapWidth, mapHeight);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(HeightMap heightMap)
    {
        int width = heightMap.values.GetLength(0);
        int height = heightMap.values.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        //generate color map for the texture
        Color[] colourMap = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colourMap[(y * width) + x] = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(heightMap.minValue, heightMap.maxValue, heightMap.values[x, y]));
            }
        }

        return TextureFromColourMap(colourMap, width, height);
    }

    public static Texture2D TextureFromShapeGenerator(Vector2Int size, ref ShapeGenerator shapeGen)
    {
        Texture2D texture = new Texture2D(size.x, size.y);

        Color[] colourMap = new Color[size.x * size.y];

        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                colourMap[(y * size.x) + x] = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(shapeGen.elevationMinMax.Min, shapeGen.elevationMinMax.Max, shapeGen.CalculateUnscaledElevation(new Vector3(x, 0, y))));
            }
        }

        return TextureFromColourMap(colourMap, size.x, size.y);
    }
}
