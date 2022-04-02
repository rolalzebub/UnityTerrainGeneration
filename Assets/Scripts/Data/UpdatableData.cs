using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatableData : ScriptableObject
{
    public event System.Action OnValuesUpdated;
    public bool autoUpdate;

    public void NotifyUpdatedValues()
    {
        if(OnValuesUpdated != null)
        {
            OnValuesUpdated();
        }
    }

    protected virtual void OnValidate()
    {
        if (autoUpdate)
        {
            NotifyUpdatedValues();
        }
    }
}
