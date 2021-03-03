using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class GameUtils
{
    /// <summary>
    /// Create an asset at path. This will also save and refresh so use CreateAssetFiles if creat multiple assets.
    /// </summary>
    /// <param name="obj">The object to create asset from</param>
    /// <param name="path">The path relative to the project, must start with "Assets/" and has a extension</param>
    /// <param name="focus">Whether to focus project window and change the active selection to the new obj</param>
    public static void CreateAssetFile(Object obj, string path, bool focus = false)
    {
        AssetDatabase.CreateAsset(obj, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (focus)
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = obj; 
        }
    }

    public static string CreateUniquePath(string path)
    {
        string dir = Path.GetDirectoryName(path);
        string fileName = Path.GetFileNameWithoutExtension(path);
        string fileExt = Path.GetExtension(path);

        for (int i = 1; ; ++i)
        {
            if (!File.Exists(path))
                return path;

            path = Path.Combine(dir, fileName, " ", i.ToString(), fileExt);
        }
    }

    public static void DrawGLTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        GL.PushMatrix();
        GL.LoadOrtho();
        GL.Begin(GL.TRIANGLES);
        GL.Vertex(a);
        GL.Vertex(b);
        GL.Vertex(c);
        GL.End();
        GL.PopMatrix();
    }

    public static void DrawGLLine(Vector3 a, Vector3 b)
    {
        DrawGLLine(a, b, Color.white);
    }

    public static void DrawGLLine(Vector3 a, Vector3 b, Color color)
    {
        GL.PushMatrix();
        GL.LoadOrtho();
        GL.Begin(GL.LINES);
        GL.Color(color);
        GL.Vertex(a);
        GL.Vertex(b);
        GL.End();
        GL.PopMatrix();
    }

    public static void Clear(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            Object.Destroy(child.gameObject);
        }
    }

    public static IEnumerator Deactive(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }

    // Use this to destroy in the OnValidate or in the editor
    public static IEnumerator DestroyInEditor(GameObject go)
    {
        yield return new WaitForEndOfFrame();
        Object.DestroyImmediate(go);
    }
}
