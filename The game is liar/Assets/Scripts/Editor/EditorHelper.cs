using UnityEditor;
using UnityEngine;
using System.IO;
using System;

public class EditorGUIHelper
{
    public static GUIStyle boxStyle;
    
    public static void DrawRect(float x, float y, float width, float height, Color color, string text = null)
    {
        DrawRect(new Rect(x, y, width, height), color, text);
    }
    
    public static void DrawRect(Rect rect, Color color, string text = null)
    {
        DrawRect(rect, color, new GUIContent(text));
    }
    
    public static void DrawRect(Rect rect, Color color, GUIContent content = null)
    {
        if (boxStyle == null)
        {
            boxStyle = GUI.skin.box;
            boxStyle.normal.background = Texture2D.whiteTexture;
        }
        Color backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = color;
        GUI.Box(rect, content ?? GUIContent.none, boxStyle);
        GUI.backgroundColor = backgroundColor;
    }
    
    public static void DrawOutline(Rect rect, float outlineSize, Color color)
    {
        DrawRect(rect.x, rect.y, rect.width, outlineSize, color);
        DrawRect(rect.x, rect.y, outlineSize, rect.height, color);
        DrawRect(rect.x, rect.y + rect.height, rect.width, outlineSize, color);
        DrawRect(rect.x + rect.width, rect.y, outlineSize, rect.height + outlineSize, color);
    }
}

public static class EditorUtils
{
    public static SerializedProperty GetParent(this SerializedProperty aProperty)
    {
        var path = aProperty.propertyPath;
        int i = path.LastIndexOf('.');
        if (i < 0)
            return null;
        return aProperty.serializedObject.FindProperty(path.Substring(0, i));
    }
    
    public static SerializedProperty FindSiblingProperty(this SerializedProperty aProperty, string aPath)
    {
        var parent = aProperty.GetParent();
        if (parent == null)
            return aProperty.serializedObject.FindProperty(aPath);
        return parent.FindPropertyRelative(aPath) ?? aProperty.serializedObject.FindProperty(aPath);
    }
    
    public static void ReadAndProcessFile<T>(string name, char separator, string folderName, Func<string[], T> action, Func<T, string> getObjectName) where T : UnityEngine.Object
    {
        try
        {
            using (FileStream stream = File.Open(name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                reader.ReadLine();
                string fileData;
                while ((fileData = reader.ReadLine()) != null)
                {
                    T obj = action(fileData.Split(separator));
                    string path = folderName + getObjectName(obj) + ".asset";
                    if (!AssetDatabase.Contains(obj))
                        AssetDatabase.CreateAsset(obj, path);
                    Debug.Log($"Done creating at {path}!");
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"The file {name} could not be read!");
            Debug.LogError(e);
            throw;
        }
    }
}
