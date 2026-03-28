using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainPainter))]
public class TerrainPainterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var painter = (TerrainPainter)target;

        if (GUILayout.Button("Pittura Terreno"))
        {
            painter.Paint();
        }

        base.OnInspectorGUI();
    }
}
