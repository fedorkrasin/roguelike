using System.Collections;
using System.Collections.Generic;
using _Core.Scripts;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomGenerator))]
public class RoomGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var generator = (RoomGenerator) target;
        
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
