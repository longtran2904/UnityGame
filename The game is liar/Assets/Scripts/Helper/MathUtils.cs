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
        Vector2Int size = Abs(oppositeCorner - conner);

        return new BoundsInt((Vector3Int)conner, (Vector3Int)size);
    }
    #endregion

    public static bool RandomBool()
    {
        return Random.value > 0.5f;
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
            Debug.LogError("min value is larger then max value!");
        }
        value.x = Mathf.Clamp(value.x, min.x, max.x);
        value.y = Mathf.Clamp(value.y, min.y, max.y);
        return value;
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
            Debug.LogError("min value is larger then max value!");
        }
        value.x = Mathf.Clamp(value.x, min.x, max.x);
        value.y = Mathf.Clamp(value.y, min.y, max.y);
        return value;
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

    public static float Square(float a)
    {
        return a * a;
    }

    public static int Square(int a)
    {
        return a * a;
    }
}
