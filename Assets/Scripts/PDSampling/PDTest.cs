using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PDTest : MonoBehaviour
{
    public float displayRadius = 1f;
    public float radius = 1f;
    public Vector2 regionSize = Vector2.one;
    public int rejectionSampleSize = 30;

    List<Vector2> points;
    private void OnValidate()
    {
        points = PDSampling.GeneratePoints(radius, regionSize, rejectionSampleSize);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(regionSize / 2, regionSize);
        if(points != null)
        {
            foreach(var point in points)
            {
                Gizmos.DrawSphere(point, displayRadius);
            }
        }
    }
}
