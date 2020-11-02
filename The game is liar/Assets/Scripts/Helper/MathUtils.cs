using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
    #region Create Bounds
    public static Bounds CreateBounds(Vector2 conner, Vector2 oppositeCorner)
    {
        Vector2 center = (conner + oppositeCorner) * .5f;

        Vector2 size = conner - oppositeCorner;
        size.x = Mathf.Abs(size.x);
        size.y = Mathf.Abs(size.y);

        return new Bounds(center, size);
    }

    public static BoundsInt CreateBoundsInt(Vector2Int conner, Vector2Int oppositeCorner)
    {
        Vector2Int size = oppositeCorner - conner;

        return new BoundsInt((Vector3Int)conner, (Vector3Int)size);
    }
    #endregion

    public static Vector2 RandomVector2(Vector2 min, Vector2 max)
    {
        return new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
    }


    // If offset == 0 then the first point is on the most right
    public static Vector2[] GeneratePointsOnCircle(Vector2 center, int numberOfPoints, float radius = 1, float offset = 0)
    {
        Vector2[] points = new Vector2[numberOfPoints];
        float deltaAngle = 360f / numberOfPoints * Mathf.Deg2Rad + offset;
        for (int i = 0; i < numberOfPoints; i++)
        {
            float angle = i * deltaAngle;
            points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }
        return points;
    }

    public static bool RandomBool()
    {
        return Random.value <= 0.5f;
    }

    public static bool RandomBool(float prob)
    {
        prob = Mathf.Clamp(prob, 0, 1);
        return Random.value < prob;
    }

    public static int UnSigned(int num)
    {
        if (num < 0)
        {
            return 0;
        }

        return num;
    }

    public static float UnSigned(float num)
    {
        if (num < 0)
        {
            return 0;
        }

        return num;
    }

    public static Vector2Int UnSigned(Vector2Int v)
    {
        if (v.x < 0)
        {
            v.x = 0;
        }

        if (v.y < 0)
        {
            v.y = 0;
        }

        return v;
    }

    public static Vector2 UnSigned(Vector2 v)
    {
        if (v.x < 0)
        {
            v.x = 0;
        }

        if (v.y < 0)
        {
            v.y = 0;
        }

        return v;
    }

    public static bool isSameSign(int a, int b)
    {
        return (a * b) > 0;
    }

    public static bool isSameSign(float a, float b)
    {
        return (a * b) > 0;
    }

    public static int Signed(int a)
    {
        if (a < 0) return -1;
        if (a > 0) return 1;
        return 0;
    }

    public static float Signed(float a)
    {
        if (a < 0) return -1;
        if (a > 0) return 1;
        return 0;
    }

    public static Vector2 Signed(Vector2 a)
    {
        if (a.x < 0f) a.x = -1;
        if (a.x > 0f) a.x =  1;
        if (a.y < 0f) a.y = -1;
        if (a.y > 0f) a.y =  1;
        return a;
    }

    #region Vector2Int
    public static Vector2Int ToVector2Int(Vector2 v, bool rounded = false)
    {
        if (rounded)
        {
            return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
        }

        return new Vector2Int((int)v.x, (int)v.y);
    }

    public static Vector2Int ToVector2Int(Vector3Int v, bool rounded = false)
    {
        if (rounded)
        {
            return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
        }

        return new Vector2Int(v.x, v.y);
    }

    public static Vector2Int Abs(Vector2Int v)
    {
        return new Vector2Int(Mathf.Abs(v.x), Mathf.Abs(v.y));
    }

    public static Vector2Int Clamp(Vector2Int value, Vector2Int min, Vector2Int max)
    {
        if (min.x > max.x || min.y > max.y)
        {
            InternalDebug.LogError("min value is larger then max value!");
        }
        value.x = Mathf.Clamp(value.x, min.x, max.x);
        value.y = Mathf.Clamp(value.y, min.y, max.y);
        return value;
    }

    public static Vector2Int Average(Vector2Int a, Vector2Int b)
    {
        return (a + b) / 2;
    }
    #endregion

    #region Vector2
    public static Vector2 Abs(Vector2 v)
    {
        return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
    }

    public static float sqrDistance(Vector2 a, Vector2 b)
    {
        return (a - b).sqrMagnitude;
    }

    public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max)
    {
        if (min.x > max.x || min.y > max.y)
        {
            InternalDebug.LogError("min value is larger then max value!");
        }
        value.x = Mathf.Clamp(value.x, min.x, max.x);
        value.y = Mathf.Clamp(value.y, min.y, max.y);
        return value;
    }

    public static Vector2 Average(Vector2 a, Vector2 b)
    {
        return (a + b) / 2;
    }
    #endregion

    public static bool InRange(int min, int max, int value)
    {
        if ((value >= min) && (value <= max))
        {
            return true;
        }
        return false;
    }

    public static bool InRange(float min, float max, float value)
    {
        if ((value >= min) && (value <= max))
        {
            return true;
        }
        return false;
    }

    public static float Sqr(float a)
    {
        return a * a;
    }

    public static int Sqr(int a)
    {
        return a * a;
    }

    public static int Average(int a, int b)
    {
        return (a + b) / 2;
    }

    public static float Average(float a, float b)
    {
        return (a + b) / 2;
    }

    public static Vector2 GetBQCPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector2 p = (uu * p0) + (2 * u * t * p1) + (tt * p2);
        return p;
    }

    #region Drag Caculation
    public static float GetDrag(float aVelocityChange, float aFinalVelocity)
    {
        return aVelocityChange / ((aFinalVelocity + aVelocityChange) * Time.fixedDeltaTime);
    }
    public static float GetDragFromAcceleration(float aAcceleration, float aFinalVelocity)
    {
        return GetDrag(aAcceleration * Time.fixedDeltaTime, aFinalVelocity);
    }
    #endregion
}
