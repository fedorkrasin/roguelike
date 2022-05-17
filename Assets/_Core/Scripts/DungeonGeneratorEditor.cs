using _Core.Scripts;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var generator = (DungeonGenerator) target;
        
        GUILayout.Space(20);

        if (GUILayout.Button("Generate Rooms"))
        {
            generator.Start();
        }
        
        if (GUILayout.Button("Clear Map"))
        {
            generator.ClearMap();
        }
    }
}
