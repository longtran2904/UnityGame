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

    public static int PositiveNumber(int num)
    {
        if (num < 0)
        {
            return 0;
        }

        return num;
    }

    public static float PositiveNumber(float num)
    {
        if (num < 0)
        {
            return 0;
        }

        return num;
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
    #endregion

    #region Vector2
    public static Vector2 Abs(Vector2 v)
    {
        return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
    }

    public static Vector2Int Abs(Vector2Int v)
    {
        return new Vector2Int(Mathf.Abs(v.x), Mathf.Abs(v.y));
    }
    #endregion
}
