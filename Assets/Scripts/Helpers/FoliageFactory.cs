using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FoliageFactory
{
    static FoliageSettings currentFoliageSettings;

    public static void SetFoliageSettings(FoliageSettings settings)
    {
        currentFoliageSettings = settings;
    }

    public static GameObject GetFoliageForPoint(FoliagePointProfile pointProfile)
    {
        GameObject toReturn = null;

        for (int i = 0; i < currentFoliageSettings.elevationLevels.Length - 1; i++)
        {
            //var ScaledElevation = pointProfile.pointElevation * pointProfile.elevationMinMax.Max;

            if (pointProfile.pointElevation >= currentFoliageSettings.elevationLevels[i] && pointProfile.pointElevation <= currentFoliageSettings.elevationLevels[i+1])
            {
                int objectCheckRange = currentFoliageSettings.elevationLevelObjectIndices[i + 1] - currentFoliageSettings.elevationLevelObjectIndices[i];
                if(objectCheckRange == 1)
                {
                    toReturn = currentFoliageSettings.foliageObjects[currentFoliageSettings.elevationLevelObjectIndices[i]];
                }
                else
                {
                    int choice = UnityEngine.Random.Range(0, objectCheckRange);
                    toReturn = currentFoliageSettings.foliageObjects[currentFoliageSettings.elevationLevelObjectIndices[i + choice]];
                }
            }
        }

        return toReturn;
    }
}
