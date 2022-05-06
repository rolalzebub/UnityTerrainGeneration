using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapPreview))]
public class MapPreviewEditor : Editor
{
    MapPreview preview;
    Editor meshSettingsEditor;
    Editor textureDataEditor;
    Editor noiseLayerSettings;
    Editor heightSettingsEditor;

    private void OnEnable()
    {
        preview = (MapPreview)target;
    }

    public override void OnInspectorGUI()
    {
        if(DrawDefaultInspector())
        {
            if (preview.autoUpdate)
            {
                preview.OnValuesUpdated();
            }
        }

        if(GUILayout.Button("Generate"))
        {
            preview.DrawMapInEditor();
        }

        DrawSettingsEditor(preview.meshSettings, preview.OnValuesUpdated, ref preview.meshSettingsFoldout, ref meshSettingsEditor);
        DrawSettingsEditor(preview.textureData, preview.OnValuesUpdated, ref preview.textureDataFoldout, ref textureDataEditor);
        DrawSettingsEditor(preview.heightSettings, preview.OnValuesUpdated, ref preview.heightSettingsFoldout, ref heightSettingsEditor);
        DrawSettingsEditor(preview.heightSettings.noiseLayerSettings, preview.OnValuesUpdated, ref preview.shapeSettingsFoldout, ref noiseLayerSettings);
    }

    void DrawSettingsEditor(Object settings, System.Action updateCallback, ref bool foldOut, ref Editor editor)
    {
        if (settings == null)
            return;

        foldOut = EditorGUILayout.InspectorTitlebar(foldOut, settings);

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            if (foldOut)
            {
                CreateCachedEditor(settings, null, ref editor);
                editor.OnInspectorGUI();

                if (check.changed)
                {
                    if (updateCallback != null)
                    {
                        updateCallback();
                    }
                }
            }
        }
    }
}
