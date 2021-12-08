using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
    #region Bounds
    public static Bounds CreateBounds(Vector2 conner, Vector2 oppositeCorner)
    {
        Vector2 center = (conner + oppositeCorner) * .5f;
        Vector2 size = oppositeCorner - conner;
        return new Bounds(center, size);
    }

    public static BoundsInt CreateBoundsInt(Vector2Int conner, Vector2Int oppositeCorner)
    {
        Vector2Int size = oppositeCorner - conner;
        return new BoundsInt((Vector3Int)conner, (Vector3Int)size);
    }

    public static Bounds ToBounds(this BoundsInt bounds)
    {
        return new Bounds((Vector3)(bounds.min + bounds.max) * .5f, bounds.size);
    }

    public static List<Vector3Int> GetOutlinePoints(this BoundsInt bounds)
    {
        List<Vector3Int> result = new List<Vector3Int>(bounds.size.x * 2 + (bounds.size.y - 2) * 2);

        for (int y = 1; y < bounds.size.y - 1; y++)
        {
            result.Add(new Vector3Int(bounds.xMin, bounds.yMin + y, 0));
            result.Add(new Vector3Int(bounds.xMax-1, bounds.yMin + y, 0));
        }

        for (int x = 0; x < bounds.size.x; x++)
        {
            result.Add(new Vector3Int(bounds.xMin + x, bounds.yMin, 0));
            result.Add(new Vector3Int(bounds.xMin + x, bounds.yMax-1, 0));
        }

        return result;
    }
    #endregion

    #region Int
    public static int UnSigned(int num)
    {
        if (num < 0)
        {
            return 0;
        }

        return num;
    }

    public static int Average(int a, int b)
    {
        return (a + b) / 2;
    }

    public static bool isSameSign(int a, int b)
    {
        return (a * b) > 0;
    }

    public static bool InRange(int min, int max, int value)
    {
        if ((value >= min) && (value <= max))
            return true;
        return false;
    }
    #endregion

    #region Float
    public static float UnSigned(float num)
    {
        if (num < 0)
        {
            return 0;
        }

        return num;
    }

    public static bool isSameSign(float a, float b)
    {
        return (a * b) > 0;
    }

    public static bool InRange(float min, float max, float value)
    {
        if ((value >= min) && (value <= max))
        {
            return true;
        }
        return false;
    }

    public static float Average(float a, float b)
    {
        return (a + b) / 2;
    }
    #endregion

    #region Vector2Int
    public static Vector2Int ToVector2Int(this Vector2 v, bool rounded = false)
    {
        if (rounded)
        {
            return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
        }

        return new Vector2Int((int)v.x, (int)v.y);
    }

    public static Vector2Int ToVector2Int(this Vector3Int v)
    {
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

    public static Vector2Int[] ToVector2Int(this Vector3Int[] v3)
    {
        return System.Array.ConvertAll(v3, ToVector2Int);
    }
    #endregion

    #region Vector2
    public static bool InRange(Vector2 center, Vector2 pos, float range)
    {
        return (pos.x < center.x + range && pos.x > center.x - range) &&
            (pos.y < center.y + range && pos.y > center.y - range);
    }

    /// <param name="degree">In degree, the function will automatically convert it to radian</param>
    public static Vector2 MakeVector2(float degree, float magnitude = 1)
    {
        float angle = degree * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * magnitude;
    }

    public static Vector2 RandomVector2()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
    }

    public static Vector2 RandomVector2(Vector2 min, Vector2 max)
    {
        return new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
    }

    public static Vector2 Abs(Vector2 v)
    {
        return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
    }

    public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max)
    {
        value.x = Mathf.Clamp(value.x, min.x, max.x);
        value.y = Mathf.Clamp(value.y, min.y, max.y);
        return value;
    }

    public static Vector2 Average(Vector2 a, Vector2 b)
    {
        return (a + b) / 2;
    }

    public static Vector2 Sign(Vector2 a)
    {
        if (a.x < 0f) a.x = -1;
        if (a.x > 0f) a.x =  1;
        if (a.y < 0f) a.y = -1;
        if (a.y > 0f) a.y =  1;
        return a;
    }

    public static Vector2 UnSigned(Vector2 v)
    {
        if (v.x < 0) v.x = 0;
        if (v.y < 0) v.y = 0;
        return v;
    }

    public static Vector2[] ToVector2(this Vector3[] v)
    {
        return System.Array.ConvertAll(v, x => (Vector2)x);
    }

    public static float DistanceLineSegmentPoint(Vector2 point, Vector2 startLine, Vector2 endLine)
    {
        // If startLine == endLine line segment is a point and will cause a divide by zero in the line segment test.
        // Instead return distance from a
        if (startLine == endLine)
            return Vector2.Distance(startLine, point);

        // Line segment to point distance equation
        Vector2 line = endLine - startLine;
        Vector2 pointToStart = startLine - point;
        return (pointToStart - line * (Vector2.Dot(pointToStart, line) / Vector2.Dot(line, line))).magnitude;
    }

    /// <param name="angle">The angle to rotate to in radiant</param>
    public static Vector2 Rotate(Vector2 v, float angle)
    {
        return new Vector2(
        v.x * Mathf.Cos(angle) - v.y * Mathf.Sin(angle),
        v.x * Mathf.Sin(angle) + v.y * Mathf.Cos(angle)
    );
    }

    /// <summary>
    /// Shorthand for performing Inverse Lerp on both x, y of a vector.
    /// </summary>
    /// <returns>The percent of value between start and end (Clamped between [(0, 0), (1, 1)])</returns>
    public static Vector2 InverseLerp(Vector2 start, Vector2 end, Vector2 value)
    {
        return (value - start) / (end - start);
    }
    #endregion

    public static Vector3 Abs(Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    #region Vector3Int
    public static Vector3Int ToVector3Int(this Vector3 v)
    {
        return new Vector3Int((int)v.x, (int)v.y, (int)v.z);
    }

    public static Vector3Int ToVector3Int(this Vector2Int v)
    {
        return new Vector3Int(v.x, v.y, 0);
    }

    public static Vector3Int[] ToVector3Int(this Vector2Int[] v)
    {
        return System.Array.ConvertAll(v, ToVector3Int);
    }
    #endregion

    #region Rect
    public static Vector2 TopLeft(this Rect rect)
    {
        return new Vector2(rect.xMin, rect.yMin);
    }

    public static Vector2 TopRight(this Rect rect)
    {
        return new Vector2(rect.xMax, rect.yMin);
    }

    public static Vector2 BottomLeft(this Rect rect)
    {
        return new Vector2(rect.xMin, rect.yMax);
    }

    public static Vector2 BottomRight(this Rect rect)
    {
        return new Vector2(rect.xMax, rect.yMax);
    }

    public static Rect ScaleSizeBy(this Rect rect, float scale)
    {
        return rect.ScaleSizeBy(scale, rect.center);
    }

    public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
    {
        Rect result = rect;
        result.x -= pivotPoint.x;
        result.y -= pivotPoint.y;
        result.xMin *= scale;
        result.xMax *= scale;
        result.yMin *= scale;
        result.yMax *= scale;
        result.x += pivotPoint.x;
        result.y += pivotPoint.y;
        return result;
    }

    public static Rect ScaleSizeBy(this Rect rect, Vector2 scale)
    {
        return rect.ScaleSizeBy(scale, rect.center);
    }

    public static Rect ScaleSizeBy(this Rect rect, Vector2 scale, Vector2 pivotPoint)
    {
        Rect result = rect;
        result.x -= pivotPoint.x;
        result.y -= pivotPoint.y;
        result.xMin *= scale.x;
        result.xMax *= scale.x;
        result.yMin *= scale.y;
        result.yMax *= scale.y;
        result.x += pivotPoint.x;
        result.y += pivotPoint.y;
        return result;
    }
    #endregion

    #region Math Calculation
    /// <summary>
    /// Rounds a given number to the given nearest number.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="nearest"></param>
    /// <returns></returns>
    public static int Round(float i, int nearest)
    {
        return (int)System.Math.Round(i / (double)nearest) * nearest;
    }

    /// <summary>
    /// Rounds a given vector to the given nearest number.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="nearest"></param>
    /// <returns></returns>
    public static Vector2 Round(Vector2 position, int nearest)
    {
        return new Vector2(Round(position.x, nearest), Round(position.y, nearest));
    }

    public static Vector2 QuadraticCurve(float t, Vector2 start, Vector2 curvenPoint, Vector2 end)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector2 p = (uu * start) + (2 * u * t * curvenPoint) + (tt * end);
        return p;
    }

    public static Vector2 CubicCurve(float t, Vector2 start, Vector2 p0, Vector2 p1, Vector2 end)
    {
        float u = 1 - t;
        return (u*u*u)*start + 3*(u*u)*t*p0 + 3*u*(t*t)*p1 + (t*t*t)*end;
    }

    // If offset == 0 then the first point is on the most right and it's anti-clockwise
    public static void GenerateCircleOutlineNonAlloc(Vector3 center, float radius, Vector3[] posArray)
    {
        float deltaDegree = 360f / posArray.Length;
        for (int i = 0; i < posArray.Length; i++)
        {
            posArray[i] = center + (Vector3)MakeVector2(i * deltaDegree) * radius;
        }
    }
    #endregion

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

    #region Random
    public static T PopRandom<T>(this List<T> list)
    {
        int index = Random.Range(0, list.Count);
        T item = list[index];
        list.RemoveAt(index);
        return item;
    }

    public static T RandomElement<T>(this List<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    public static T RandomElement<T>(this T[] array)
    {
        return array[Random.Range(0, array.Length)];
    }

    public static int RandomSign()
    {
        return RandomBool() ? 1 : -1;
    }

    public static bool RandomBool()
    {
        return Random.value <= 0.5f;
    }

    /// <param name="prob">The probability of return true. Clamp between 0-1</param>
    public static bool RandomBool(float prob)
    {
        prob = Mathf.Clamp(prob, 0, 1);
        return Random.value <= prob;
    }

    /// <summary>
    /// Choose random elements of an array. This will create and return a new array which can has memory allocation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="numRequired">The number of elements to choose. If this is greater than the array's length then all the array will be choose.</param>
    /// <param name="array">The input array to copy from.</param>
    /// <returns>Return a new array that contain all the chosen elements.</returns>
    public static T[] ChooseSet<T>(int numRequired, T[] array)
    {
        if (numRequired >= array.Length)
            return (T[])array.Clone();

        T[] result = new T[numRequired];
        int numToChoose = numRequired;
        for (int numLeft = array.Length; numLeft > 0; numLeft--)
        {
            if (numLeft <= numToChoose)
            {
                System.Array.Copy(array, 0, result, 0, numLeft);
            }

            float prob = (float)numToChoose / numLeft;
            if (Random.value <= prob)
            {
                numToChoose--;
                result[numToChoose] = array[numLeft - 1];
                if (numToChoose == 0)
                    break;
            }
        }
        return result;
    }

    /// <summary>
    /// Choose random elements of an array and copy it to the results array. This is more efficient than ChooseSet because it doesn't has any memory allocation.
    /// Note that it doesn't resize the results array so when it filled up the results array it will return.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="numRequired">The number of elements to choose. If this is greater than the array's length then all the array will be choose.</param>
    /// <param name="array">The array to copy from.</param>
    /// <param name="results">The array to copy the results to.</param>
    /// <returns>The number of chosen elements. This will be the input array's length if numRequired is greater than the array's length.</returns>
    public static int ChooseSetNonAlloc<T>(int numRequired, T[] array, T[] results)
    {
        // When numRequired >= array.length
        if (numRequired >= array.Length)
        {
            for (int i = 0; i < results.Length; i++)
            {
                if (i == array.Length)
                    return i;
                results[i] = array[i];
            }
            return results.Length;
        }

        int numToChoose = numRequired;
        int numHas = 0; // The number of chosen elements. This is the return value
        int offset = Mathf.Clamp(numToChoose - results.Length, 0, numToChoose); // offset of the index if numToChooose is greater than results.Length

        for (int numLeft = array.Length; numLeft > 0; numLeft--)
        {
            // When numLeft <= numToChoose just copy all the remaining elements and return
            if (numLeft <= numToChoose)
            {
                for (int i = numLeft - 1; i >= 0; i--)
                {
                    // This is when the results array has been filled up
                    if (i - offset <= 0)
                        return numHas + numLeft - i;
                    results[i - offset] = array[i];
                }
                return numHas + numLeft;
            }

            float prob = (float)numToChoose / numLeft;
            if (Random.value <= prob)
            {
                numToChoose--;
                int resultsIndex = numToChoose - offset;

                if (resultsIndex == 0) // This is when the results array has been filled up
                {
                    results[resultsIndex] = array[numLeft - 1];
                    return numHas++;
                }

                results[resultsIndex] = array[numLeft - 1];
                if (numToChoose == 0)
                    break;
                numHas++;
            }
        }
        return numHas;
    }

    /// <summary>
    /// Return a random index to the array
    /// </summary>
    public static int Choose(float[] probs)
    {
        float total = 0;
        foreach (float elem in probs)
        {
            total += elem;
        }
        float randomPoint = Random.value * total;
        for (int i = 0; i < probs.Length; i++)
        {
            if (randomPoint < probs[i])
                return i;
            else
                randomPoint -= probs[i];
        }
        return probs.Length - 1;
    }

    public static void Shuffle<T>(T[] deck)
    {
        for (int i = 0; i < deck.Length; i++)
        {
            T temp = deck[i];
            int randomIndex = Random.Range(0, deck.Length);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }
    #endregion

    public static T[] Populate<T>(this T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
        return array;
    }

    public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
    {
        while (enumerator.MoveNext())
            yield return enumerator.Current;
    }

    public static void Swap<T>(ref T a, ref T b)
    {
        T temp = a;
        a = b;
        b = temp;
    }
}
