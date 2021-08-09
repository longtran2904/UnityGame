using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurveVisualizer : MonoBehaviour
{
    public Vector2 start;
    public Vector2 end;
    public RangedFloat aX;
    public RangedFloat aY;
    public RangedFloat bX;
    public RangedFloat bY;
    public float yMutiplier;
    public int numberOfPointsBetween;
    Vector2[] points;

    [EasyButtons.Button]
    public void DrawCurve()
    {
        float delta = 1f / (numberOfPointsBetween + 1);
        Vector2 p0 = start + new Vector2(aX.randomValue, aY.randomValue * yMutiplier) * (end - start).magnitude / 2;
        Vector2 p1 = end   - new Vector2(bX.randomValue, bY.randomValue * yMutiplier) * (end - start).magnitude / 2;
        points = new Vector2[numberOfPointsBetween + 2];
        for (int i = 0; i < numberOfPointsBetween + 2; i++)
        {
            points[i] = MathUtils.CubicCurve(delta * i, start, p0, p1, end);
        }
    }

    public void OnDrawGizmos()
    {
        if (points.Length == 0) return;
        for (int i = 1; i < points.Length; i++)
        {
            Gizmos.DrawLine(points[i - 1], points[i]);
        }
    }
}
