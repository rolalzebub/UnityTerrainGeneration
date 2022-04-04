using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatableData : ScriptableObject
{
    public event System.Action OnValuesUpdated;
    public bool autoUpdate;

#if UNITY_EDITOR
    public void NotifyUpdatedValues()
    {
        UnityEditor.EditorApplication.update -= NotifyUpdatedValues;

        if (OnValuesUpdated != null)
        {
            OnValuesUpdated();
        }
    }

    protected virtual void OnValidate()
    {
        if (autoUpdate)
        {
            UnityEditor.EditorApplication.update += NotifyUpdatedValues;
        }
    }
#endif
}
