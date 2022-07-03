using System.Diagnostics;
using UnityEngine;

public static class GameDebug
{
    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void DrawBox(Vector2 center, Vector2 size, Color color)
    {
        Vector2 extents = size / 2;
        Vector2 topLeft = center + new Vector2(-extents.x, extents.y);
        Vector2 topRight = center + extents;
        Vector2 bottomLeft = center - extents;
        Vector2 bottomRight = center + new Vector2(extents.x, -extents.y);

        UnityEngine.Debug.DrawLine(topLeft, topRight, color);
        UnityEngine.Debug.DrawLine(topLeft, bottomLeft, color);
        UnityEngine.Debug.DrawLine(bottomRight, bottomLeft, color);
        UnityEngine.Debug.DrawLine(bottomRight, topRight, color);
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void DrawBounds(Bounds bounds, Color color)
    {
        DrawBox(bounds.center, bounds.size, color);
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void DrawBox(RectInt rect, Color color)
    {
        DrawBox(rect.center, rect.size, color);
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void DrawBox(Rect rect, Color color)
    {
        DrawBox(rect.center, rect.size, color);
    }

    public static void DrawBoxMinMax(Vector2 min, Vector2 max, Color color)
    {
        DrawBox((min + max) / 2, max - min, color);
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void DrawCircle(Vector2 pos, float radius, Color color)
    {
        const int maxPoints = 512;
        float rotPerPoint = 360f / maxPoints;
        for (int i = 0; i < maxPoints; i++)
            UnityEngine.Debug.DrawLine(pos + MathUtils.MakeVector2(rotPerPoint * i, radius), pos + MathUtils.MakeVector2(rotPerPoint * (i + 1), radius), color);
    }
}
