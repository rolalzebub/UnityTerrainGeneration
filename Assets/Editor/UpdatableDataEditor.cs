using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UpdatableData), true)]
public class UpdatableDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UpdatableData data = target as UpdatableData;

        if(!data.autoUpdate)
        {
            if(GUILayout.Button("Update"))
            {
                data.NotifyUpdatedValues();
            }
        }
    }
}
