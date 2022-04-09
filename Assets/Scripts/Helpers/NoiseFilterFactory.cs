using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseFilterFactory
{
    public static INoiseFilter CreateNoiseFilter(SimplexNoiseSettings settings)
    {
        switch(settings.filterType)
        {
            case SimplexNoiseSettings.FilterType.Simple:
                return new SimpleNoiseFilter(settings.simpleNoiseSettings);

            case SimplexNoiseSettings.FilterType.Ridged:
                return new RidgedNoiseFilter(settings.ridgedNoiseSettings);
        }

        return null;
    }
}
