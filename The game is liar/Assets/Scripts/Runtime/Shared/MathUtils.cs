using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpringData
{
    [Range( 0, 20)] public float f;
    [Range( 0, 10)] public float z;
    [Range(-5,  5)] public float r;
    [HideInInspector] public float prevX, dy;
    
    public void Init(float x0)
    {
        prevX = x0;
        dy = 0;
    }
}

public enum PositionType
{
    Random,
    Min,
    Middle,
    Max,
}

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
        return new BoundsInt((Vector3Int)conner, (Vector3Int)size + Vector3Int.forward);
    }
    
    public static Bounds ToBounds(this BoundsInt bounds)
    {
        return new Bounds((Vector3)(bounds.min + bounds.max) * .5f, bounds.size);
    }
    
    public static BoundsInt ToBoundsInt(this Bounds bounds)
    {
        return new BoundsInt(bounds.min.ToVector3Int(), bounds.size.ToVector3Int());
    }
    
    public static List<Vector3Int> GetOutlinePoints(BoundsInt bounds)
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
    
    public static int Area(this BoundsInt bounds)
    {
        return bounds.size.x * bounds.size.y;
    }
#endregion
    
#region Int
    public static bool HasFlag(ulong value, int bitPos)
    {
        return (value & (1ul << bitPos)) != 0;
    }
    
    public static ulong SetFlag(ulong value, int bitPos, bool set)
    {
        if (set)
            value |= 1ul << bitPos;
        else
            value &= ~(1ul << bitPos);
        return value;
    }
    
    public static int UnSigned(int num)
    {
        if (num < 0)
            return 0;
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
    
    public static bool InRange(int value, int min, int max)
    {
        if ((value >= min) && (value <= max))
            return true;
        return false;
    }
    
    public static int LoopIndex(int nextIndex, int length, bool loop)
    {
        if (nextIndex < 0)
            return loop ? length - 1 : 0;
        else if (nextIndex >= length)
            return loop ? 0 : length - 1;
        return nextIndex;
    }
#endregion
    
#region Float
    // TODO: This may change on different platforms. We can query this value at runtime
    public const float Epsilon = 1.1920929e-7f;
    
    public static bool IsApproximate(float a, float b, float epsilon = Epsilon)
    {
        return Mathf.Abs(a - b) <= epsilon;
    }
    
    public static bool InRange(float val, float min, float max)
    {
        return val >= min && val <= max;
    }
    
    public static float MulAdd(float a, float b, bool mul, bool add)
    {
        if (mul && add)
            return a + a * b;
        else if (mul)
            return a * b;
        else if (add)
            return a + b;
        else
            return b;
    }
    
    // NOTE: Unity Mathf.Sign return 1 when a = 0
    public static float Sign(float a)
    {
        if (a > 0) return  1;
        if (a < 0) return -1;
        return 0;
    }
    
    public static float UnSigned(float num)
    {
        if (num < 0)
            return 0;
        return num;
    }
    
    public static bool isSameSign(float a, float b)
    {
        return (a * b) > 0;
    }
    
    public static bool InRange(float min, float max, float value, bool border)
    {
        if (border)
            return (value >= min) && (value <= max);
        else
            return value > min && value < max;
    }
    
    public static bool RangeInRange(float aMin, float aRange, float bMin, float bRange)
    {
        float aMax = aMin + aRange;
        float bMax = bMin + bRange;
        return (bMin >= aMin && bMin <= aMax) ||
        (bMax >= aMin && bMax <= aMax);
    }
    
    public static float Average(float a, float b)
    {
        return (a + b) / 2;
    }
#endregion
    
#region Vector2Int
    public static Vector2Int ToVector2Int(this Vector2 v, bool ceil = false)
    {
        if (ceil)
            return new Vector2Int(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y));
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
    
    public static float Dot(this Vector2Int a, Vector2Int b)
    {
        return a.x * b.x + a.y * b.y;
    }
#endregion
    
#region Vector2
    public static bool IsApproximate(Vector2 a, Vector2 b, float epsilon = Epsilon, int compareDir = 0)
    {
        float x = a.x - b.x;
        float y = a.y - b.y;
        bool isApproximate = Mathf.Abs(x) <= epsilon && Mathf.Abs(y) <= epsilon;
        bool outOfRange = compareDir != 0 ? Sign(x) == Sign(compareDir) || Sign(y) == Sign(compareDir) : true;
        return isApproximate || outOfRange;
    }
    
    public static Vector2 X(this Vector2 v, float x) => new Vector2(x, v.y);
    public static Vector2 Y(this Vector2 v, float y) => new Vector2(v.x, y);
    public static Vector3 Z(this Vector2 v, float z) => new Vector3(v.x, v.y, z);
    
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
    
    public static Vector2 RandomPointInRange(float range)
    {
        return RandomVector2().normalized * UnityEngine.Random.Range(0, range);
    }
    
    public static Vector2 RandomVector2()
    {
        return new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
    }
    
    public static Vector2 RandomPoint(Vector2 min, Vector2 max)
    {
        return new Vector2(UnityEngine.Random.Range(min.x, max.x), UnityEngine.Random.Range(min.y, max.y));
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
    
    public static Vector2 Clamp01(Vector2 value)
    {
        return Clamp(value, Vector2.zero, Vector2.one);
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
    
    public static Vector2 OnlySignX(Vector2 v) => new Vector2(Mathf.Sign(v.x), 0);
    
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
    
    public static Vector2 InverseScale(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x / b.x, a.y / b.y);
    }
    
    public static Vector2 Min(Vector2 a, Vector2 b)
    {
        return new Vector2(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
    }
    public static Vector2 Max(Vector2 a, Vector2 b)
    {
        return new Vector2(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
    }
#endregion
    
#region Vector3
    public static Vector3 Abs(Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }
    
    public static Vector3 InverseScale(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }
    
    public static Vector3 Clamp(Vector2 pos, Vector2 min, Vector2 max, float posZ)
    {
        return new Vector3(Mathf.Clamp(pos.x, min.x, max.x), Mathf.Clamp(pos.y, min.y, max.y), posZ);
    }
#endregion
    
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
    /*
    RANT: Unity doesn't document these cases, they also don't provide any API to cover the other cases.

    Rect a = new Rect(Vector2.zero, Vector2.one);
    Rect b = new Rect(Vector2.right, Vector2.one);
    Debug.Log(a.Overlaps(b));                               // False
    Debug.Log(a.Contains(Vector2.right));                   // False
    Debug.Log(a.Contains(Vector2.zero));                    // True

    RectInt aInt = new RectInt(Vector2Int.zero , Vector2Int.one);
    RectInt bInt = new RectInt(Vector2Int.right, Vector2Int.one);
    Debug.Log(aInt.Overlaps(bInt));                         // False
    Debug.Log(aInt.Contains(Vector2Int.right));             // False
    Debug.Log(aInt.Contains(Vector2Int.zero));              // True
    */
    public static Rect Resize(this Rect r, Vector2 minOffset, Vector2 maxOffset)
    {
        r.min += minOffset;
        r.max += maxOffset;
        return r;
    }
    
    public static Rect GetOverlap(Rect a, Rect b)
    {
        Vector2 minPos = Max(a.min, b.min);
        Vector2 maxPos = Min(a.max, b.max);
        return new Rect(minPos, maxPos - minPos);
    }
    
    public static Vector2 RandomPoint(this Rect rect)
    {
        float x = UnityEngine.Random.Range(rect.xMin, rect.xMax);
        float y = UnityEngine.Random.Range(rect.yMin, rect.yMax);
        return new Vector2(x, y);
    }
    
    public static Rect CreateRect(Vector2 center, Vector2 size) => new Rect(center - size / 2, size);
    
    public static Rect CreateRect(Vector2 pos, Vector2 size, float scale) => CreateRect(pos + size / 2, size * scale);
    
    public static Rect CreateRectMinMax(Vector2 min, Vector2 max) => new Rect(min, max - min);
    
    public static Rect ToRect(this RectInt r) => new Rect(r.min, r.size);
    
    public static Rect ToRect(this Bounds bounds)
    {
        return new Rect(bounds.min, bounds.size);
    }
    
    public static Rect InfiniteRect => Rect.MinMaxRect(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue);
    
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
    
    public static bool OverlapsEx(this Rect a, Rect b, bool border = false)
    {
        return a.ContainsEx(b.min, border) || a.ContainsEx(b.max, border);
    }
    
    public static bool ContainsEx(this Rect a, Vector2 pos, bool border = false)
    {
        return InRange(a.xMin, a.xMax, pos.x, border) && InRange(a.yMin, a.yMax, pos.y, border);
    }
    
    public static float CollideArea(this RectInt a, RectInt b)
    {
        float width = Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin);
        float height = Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin);
        return Mathf.Max(width * height, 0);
    }
    
    public static bool OverlapsWithoutBorder(this RectInt a, RectInt b, int maxBoundsOffset = 0)
    {
        a.max -= Vector2Int.one * maxBoundsOffset;
        b.max -= Vector2Int.one * maxBoundsOffset;
        
        bool collideX = !(a.xMax <= b.xMin || a.xMin >= b.xMax);
        bool collideY = !(a.yMax <= b.yMin || a.yMin >= b.yMax);
        return collideX && collideY;
    }
    
    public static int Area(this RectInt a)
    {
        return a.width * a.height;
    }
    
    public static BoundsInt ToBoundsInt(this RectInt a)
    {
        return new BoundsInt(a.position.ToVector3Int(), a.size.ToVector3Int() + Vector3Int.forward);
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
    
    public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, float deltaTime, float maxSpeed = float.MaxValue)
    {
        // Based on Game Programming Gems 4 Chapter 1.10
        smoothTime = Mathf.Max(0.0001F, smoothTime);
        float omega = 2F / smoothTime;
        
        float x = omega * deltaTime;
        float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);
        float change = current - target;
        float originalTo = target;
        
        // Clamp maximum speed
        float maxChange = maxSpeed * smoothTime;
        change = Mathf.Clamp(change, -maxChange, maxChange);
        target = current - change;
        
        float temp = (currentVelocity + omega * change) * deltaTime;
        currentVelocity = (currentVelocity - omega * temp) * exp;
        float output = target + (change + temp) * exp;
        
        // Prevent overshooting
        if (originalTo - current > 0.0F == output > originalTo)
        {
            output = originalTo;
            currentVelocity = (output - originalTo) / deltaTime;
        }
        
        return output;
    }
    
    // NOTE: The ref currentVelocity can be use for rb.velocity with some precision error (it can't move as fast as changing the position).
    public static Vector3 SmoothDamp(Vector2 current, Vector2 target, ref Vector2 currentVelocity, Vector2 smoothTime, float deltaTime, float posZ)
    {
        return new Vector3(
                           SmoothDamp(current.x, target.x, ref currentVelocity.x, smoothTime.x, deltaTime),
                           SmoothDamp(current.y, target.y, ref currentVelocity.y, smoothTime.y, deltaTime),
                           posZ);
    }
    
    // LINK: https://answers.unity.com/questions/1679798/how-can-you-make-gravity-to-not-have-any-accelerat.html
    public static float GetDragFromAcceleration(float accelaration, float maxVelocity)
    {
        float deltaVelocity = accelaration * Time.fixedDeltaTime;
        return deltaVelocity / ((maxVelocity + deltaVelocity) * Time.fixedDeltaTime);
    }
    
    public static float PingPong(float range, float t)
    {
        range += -((int)t % 2) * 2 * range;
        return Mathf.Lerp(-range, range, t - (int)t);
    }
    
    public static (float, float, float) InitK123(float f, float z, float r)
    {
        if (f == 0) return (0, 0, 0);
        float k1 = z / (Mathf.PI * f);
        float k2 = 1 / ((2 * Mathf.PI * f) * (2 * Mathf.PI * f));
        float k3 = r * z / (2 * Mathf.PI * f);
        return (k1, k2, k3);
    }
    
    public static float SecondOrder(float t, float k1, float k2, float k3, float x, float prevX, float y, ref float dy)
    {
        if (k2 == 0) return x; 
        
        float k2_stable = Mathf.Max(k2, t * t / 2 + t * k1 / 2, t * k1);
        y += t * dy;
        
        // NOTE: Rather than calculate dx like below, I multiply it in so if t equals to zero then dy doesn't change
        // dx = (x - prevX) / t // This will crash
        // dy += t * (x + k3 * dx - y - k1 * dy) / k2_stable
        dy += (t * (x - y - k1 * dy) + k3 * (x - prevX)) / k2_stable;
        
        return y;
    }
    
    public static float SecondOrder(float t, float x, float y, SpringData spring)
    {
        (float k1, float k2, float k3) = InitK123(spring.f, spring.z, spring.r);
        
#if false
        float k2_stable = Mathf.Max(k2, t * t / 2 + t * k1 / 2, t * k1);
        y += t * spring.dy;
        spring.dy += (t * (x - y - k1 * spring.dy) + k3 * (x - spring.prevX)) / k2_stable;
#else
        y = SecondOrder(t, k1, k2, k3, x, spring.prevX, y, ref spring.dy);
#endif
        
        spring.prevX = x;
        return y;
    }
    
    public static float GetRotZ(Vector2 dir)
    {
        return dir == Vector2.zero ? 0 : Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }
    
    public static Quaternion LookRotation(Vector3 lookDir, Vector3 forward)
    {
        return Quaternion.LookRotation(forward, Vector3.Cross(forward, lookDir));
    }
#endregion
    
#region Fast and Funky 1D Nonlinear Transformations
    public static float SmoothStart2(float t)
    {
        return t * t;
    }
    
    public static float SmoothStart3(float t)
    {
        return t * t * t;
    }
    
    public static float SmoothStart4(float t)
    {
        return t * t * t * t;
    }
    
    public static float SmoothStop2(float t)
    {
        return Flip(SmoothStart2(Flip(t))); // 1 - (1 - t)^2
    }
    
    public static float SmoothStop3(float t)
    {
        return Flip(SmoothStart3(Flip(t)));
    }
    
    public static float SmoothStop4(float t)
    {
        return Flip(SmoothStart4(Flip(t)));
    }
    
    public static float Arch2(float t)
    {
        return Scale(Flip(t), t); // t * (1 - t)
    }
    
    public static float SmoothStartArch3(float t)
    {
        return Scale(Arch2(t), t); // t^2 * (1 - t)
    }
    
    public static float SmoothStopArch3(float t)
    {
        return ReverseSclae(Arch2(t), t); // t * (1 - t)^2
    }
    
    public static float SmoothStepArch4(float t)
    {
        return ReverseSclae(Scale(Arch2(t), t), t); // t^2 * (1 - t)^2
    }
    
    public static float BellCurve6(float t)
    {
        return SmoothStop3(t) * SmoothStart3(t); // t^3 * (1 - (1-t)^3)
    }
    
    public static float Mix(float smoothStart, float smoothStop, float blend)
    {
        return smoothStart + blend * (smoothStop - smoothStart); // Or (1 - blend) * a + blend * b
        // Btw: Mix(SmoothStart2, SmoothStop2, .5f) = Linear
        // If the blend is t then this is called Crossfade.
        // Btw: Crossfade(SmoothStart2, SmoothStop2) = SmoothStep3
    }
    
    public static float Flip(float t)
    {
        return 1 - t;
    }
    
    public static float Scale(float func, float t)
    {
        return t * func;
    }
    
    public static float ReverseSclae(float func, float t)
    {
        return (1 - t) * func;
    }
    
    // Dino Dini’s Normalized Tunable Sigmoid Function: https://dhemery.github.io/DHE-Modules/technical/sigmoid/
    // t can be in [0,1] or [-1,1], k = 0 is linear, k > 0 is J shape, k < 0 is reverse J shape
    public static float NTSF(float t, float k)
    {
        float result = (t - k * t) / (k - 2 * k * Mathf.Abs(t) + 1);
        return result;
    }
#endregion
    
#region Random
    public static T PopRandom<T>(this List<T> list)
    {
        int index = UnityEngine.Random.Range(0, list.Count);
        T item = list[index];
        list.RemoveAt(index);
        return item;
    }
    
    public static T RandomElement<T>(this List<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }
    
    public static T RandomElement<T>(this T[] array)
    {
        return array[UnityEngine.Random.Range(0, array.Length)];
    }
    
    public static int RandomSign()
    {
        return Random() ? 1 : -1;
    }
    
    public static bool Random()
    {
        return UnityEngine.Random.value <= 0.5f;
    }
    
    /// <param name="prob">The probability of return true. Clamp between 0-1</param>
    public static bool RandomBool(float prob)
    {
        prob = Mathf.Clamp(prob, 0, 1);
        return UnityEngine.Random.value <= prob;
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
            if (UnityEngine.Random.value <= prob)
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
            if (UnityEngine.Random.value <= prob)
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
        float randomPoint = UnityEngine.Random.value * total;
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
            int randomIndex = UnityEngine.Random.Range(0, deck.Length);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }
    
    public static uint Noise(int pos, uint seed = 0)
    {
        const uint BIT_NOISE1 = 0xB5297A4D;
        const uint BIT_NOISE2 = 0x68E31DA4;
        const uint BIT_NOISE3 = 0x1B56C4E9;
        
        uint mangled = (uint)pos;
        mangled *= BIT_NOISE1;
        mangled += seed;
        mangled ^= (mangled >> 8);
        mangled += BIT_NOISE2;
        mangled ^= (mangled << 8);
        mangled *= BIT_NOISE3;
        mangled ^= (mangled >> 8);
        
        return mangled;
    }
    
    public static float NoiseZeroToOne(int pos, uint seed = 0)
    {
        return (float)Noise(pos, seed) / uint.MaxValue;
    }
    
    public static float NoiseNegOneToOne(int pos, uint seed = 0)
    {
        return (float)Noise(pos, seed) / uint.MaxValue * 2f - 1f;
    }
#endregion
    
#region Data Structres
    public static T Last<T>(this IList<T> collection)
    {
        return collection[collection.Count - 1];
    }
    
    public static T[] Populate<T>(this T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
            array[i] = value;
        return array;
    }
    
    public delegate T GetElement<T>(int index);
    public static T[] Populate<T>(this T[] array, GetElement<T> getValue)
    {
        for (int i = 0; i < array.Length; i++)
            array[i] = getValue(i);
        return array;
    }
    
    public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
    {
        while (enumerator.MoveNext())
            yield return enumerator.Current;
    }
    
    public static List<T> EmptyList<T>()
    {
        return new List<T>(0);
    }
    
    public static T[] AddNew<T>(this IList<T> a, params T[] b)
    {
        T[] result = new T[a.Count + b.Length];
        a.CopyTo(result, 0);
        b.CopyTo(result, a.Count);
        return result;
    }
    
    public static void AddRange<T, U>(this IList<T> list, ICollection<U> collection, System.Func<U, T> convert, System.Func<U, bool> canProcess = null)
    {
        if (list is List<T> realList)
            EnsureCapacity(realList, collection.Count);
        canProcess ??= _ => true;
        foreach (U item in collection)
            if (canProcess(item))
            list.Add(convert(item));
    }
    
    // NOTE: This's just a temporary function because C# doesn't have a ListSegment type like ArraySegement
    public static IList<T> GetRange<T>(this IList<T> list, int offset, int count)
    {
        IList<T> result;
        if (list is System.Array) result = new System.ArraySegment<T>((T[])list, offset, count);
        else
        {
            result = new List<T>(count);
            for (int i = offset; i < offset + count; ++i)
                result.Add(list[i]);
        }
        return result;
    }
    
    public static IList<T> AdvanceRange<T>(this IList<T> list, ref int index, int count, bool forward = false)
    {
        int oldIndex = index;
        index += count * (forward ? 1 : -1);
        return list.GetRange(forward ? oldIndex : index, count);
    }
    
    public static void EnsureCapacity<T>(this List<T> list, int count)
    {
        int free = list.Capacity - list.Count;
        if (free < count)
            list.Capacity += count - free;
    }
#endregion
    
#region Misc
    public static void Swap<T>(ref T a, ref T b)
    {
        T temp = a;
        a = b;
        b = temp;
    }
    
    public static Color ChangeAlpha(this Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }
#endregion
}
