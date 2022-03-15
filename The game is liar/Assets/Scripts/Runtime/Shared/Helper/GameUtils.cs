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

    public static string FormatCamelCase(this string str)
    {
        return System.Text.RegularExpressions.Regex.Replace(str, "([a-z](?=[A-Z]|[0-9])|[A-Z](?=[A-Z][a-z]|[0-9])|[0-9](?=[^0-9]))", "$1 ");
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

    // NOTE: This function only search for 1 level deep.
    public static GameObject FindChildWithLayer(this Transform parent, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        foreach (Transform child in parent)
            if (child.gameObject.layer == layer)
                return child.gameObject;
        return null;
    }

    public static void Clear(this Transform transform)
    {
        foreach (Transform child in transform)
            Object.Destroy(child.gameObject);
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

    public static Coroutine InvokeAfter(this MonoBehaviour behaviour, float delayTime, System.Action action)
    {
        if (behaviour != null && action != null)
            return behaviour.StartCoroutine(InvokeAfter());
        return null;

        IEnumerator InvokeAfter()
        {
            yield return new WaitForSeconds(delayTime);
            action();
        }
    }

    // TODO: Only stop time on a global gameObject that can't be destroyed.
    public static IEnumerator StopTime(float time)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(time);
        Time.timeScale = 1;
    }

    public static Coroutine EmptyCoroutine(this MonoBehaviour mb)
    {
        return mb.StartCoroutine(Empty());

        IEnumerator Empty()
        {
            yield return null;
        }
    }

    public static IEnumerator SlowdownTime(float duration, float timeScale = .1f)
    {
        float defTimeScale = Time.timeScale;
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = defTimeScale;
    }

    public static void SetMaterialBlock(SpriteRenderer sr, System.Action<MaterialPropertyBlock> func)
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        sr.GetPropertyBlock(block);
        func?.Invoke(block);
        sr.SetPropertyBlock(block);
    }

    public static Vector2 HalfSize(this Camera camera)
    {
        return new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize);
    }
}
