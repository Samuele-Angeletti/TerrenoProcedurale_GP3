using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PerlinTerrain))]
public class PerlinTerrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var terrain = (PerlinTerrain)target;

        if (GUILayout.Button("Genera Terreno"))
        {
            terrain.Generate();
        }

        GUILayout.Space(5);

        base.OnInspectorGUI();
    }
}
