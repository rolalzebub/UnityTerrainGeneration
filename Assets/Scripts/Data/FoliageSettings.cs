using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class FoliageSettings : ScriptableObject
{
    
    public GameObject[] foliageObjects;
    public int[] elevationLevelObjectIndices;
    public float[] elevationLevels;

    public float objectSeparationRadius = 1.7f;
}
