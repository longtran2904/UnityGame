using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(StructuralLevel)), CanEditMultipleObjects]
public class CustomLevelStructuralEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        StructuralLevel level = (StructuralLevel)target;

        if (GUILayout.Button(new GUIContent("Generate Level")))
        {
            var tempList = level.transform.Cast<Transform>().ToList();
            foreach (Transform child in tempList)
            {
                DestroyImmediate(child.gameObject);
            }
            level.Generate();
        }
    }
}
