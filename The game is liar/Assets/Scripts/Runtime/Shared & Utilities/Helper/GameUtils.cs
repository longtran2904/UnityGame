using System.Collections;
using System.IO;
using UnityEngine;

public static class GameUtils
{
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
        DrawGLTriangle(a, b, c, Color.white);
    }

    public static void DrawGLTriangle(Vector3 a, Vector3 b, Vector3 c, Color color)
    {
        DrawGL(GL.TRIANGLES, color, a, b, c);
    }

    public static void DrawGLLine(Vector3 a, Vector3 b)
    {
        DrawGLLine(a, b, Color.white);
    }

    public static void DrawGLLine(Vector3 a, Vector3 b, Color color)
    {
        DrawGL(GL.LINES, color, a, b);
    }

    private static void DrawGL(int mode, Color color, params Vector3[] vertices)
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.hideFlags = HideFlags.HideAndDontSave;

        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadOrtho();
        GL.Begin(mode);
        GL.Color(color);
        foreach (var vertex in vertices)
        {
            GL.Vertex(vertex);
        }
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
