using UnityEditor;
using UnityEngine;
using UnityEditor.Tilemaps;

[CustomEditor(typeof(Rooms)), CanEditMultipleObjects]
public class CustomRoomsInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Rooms room = (Rooms)target;

        room.editTileMode = EditorGUILayout.Toggle(new GUIContent("Edit Tile Mode"), room.editTileMode);        

        if (GUILayout.Button(new GUIContent("Save Tiles")))
        {
            room.SaveTile(room.serializableTiles);
        }

        EditorGUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        room.exits[0] = EditorGUILayout.Toggle(room.exits[0]);

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label(new GUIContent("Exits"), GUILayout.Width(30));

        GUILayout.Space(95);

        room.exits[1] = EditorGUILayout.Toggle(room.exits[1], GUILayout.Width(15));

        GUILayout.Space(17);

        room.exits[2] = EditorGUILayout.Toggle(room.exits[2], GUILayout.Width(15));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        room.exits[3] = EditorGUILayout.Toggle(room.exits[3]);

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();

        room.SetBoundaries();
    }
}
