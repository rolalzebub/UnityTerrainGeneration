using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
    TerrainGenerator terrGen;
    Editor meshSettingsEditor;
    Editor heightMapSettingsEditor;
    Editor textureSettingsEditor;
    Editor shapeSettingsEditor;
    MapPreview editorPreview;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DrawSettingsEditor(terrGen.heightSettings, editorPreview.OnValuesUpdated, ref terrGen.heightSettingsFoldout, ref heightMapSettingsEditor);
        DrawSettingsEditor(terrGen.meshSettings, editorPreview.OnValuesUpdated, ref terrGen.meshSettingsFoldout, ref meshSettingsEditor);
        DrawSettingsEditor(terrGen.textureSettings, editorPreview.OnValuesUpdated, ref terrGen.textureSettingsFoldout, ref textureSettingsEditor);
        DrawSettingsEditor(terrGen.shapeSettings, editorPreview.OnValuesUpdated, ref terrGen.shapeSettingsFoldout, ref shapeSettingsEditor);
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

    private void OnEnable()
    {
        terrGen = (TerrainGenerator) target;
        editorPreview = FindObjectOfType<MapPreview>();
    }
}
