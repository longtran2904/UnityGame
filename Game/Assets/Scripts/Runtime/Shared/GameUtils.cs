using System;
using System.Collections;
using System.IO;
using System.Text;

using UnityEngine;
using UnityEngine.Tilemaps;

using FieldInfo = System.Reflection.FieldInfo;

public static class GameUtils
{
#region Tilemap
    public static Tilemap CompressAndRefresh(this Tilemap tilemap)
    {
        tilemap.CompressBounds();
        tilemap.RefreshAllTiles();
        return tilemap;
    }
    
    public static Tilemap ResizeAndRefresh(this Tilemap tilemap, Vector3Int pos, Vector3Int size, int scale)
    {
        tilemap.origin = pos * scale;
        size *= scale;
        size.z = 1;
        tilemap.size = size;
        tilemap.ResizeBounds();
        tilemap.RefreshAllTiles();
        return tilemap;
    }
    
    public static Tilemap ClearAndCompress(this Tilemap tilemap)
    {
        tilemap.ClearAllTiles();
        tilemap.CompressBounds();
        return tilemap;
    }
#endregion
    
#region Reflection
    // NOTE:
    // readonly         false   true    false
    // static readonly  false   true    true
    // const            true    false   true
    public static bool IsSerializableField(FieldInfo field, bool whenNull = false) => field == null ?
        whenNull : !(field.IsLiteral || field.IsInitOnly || field.IsStatic);
    
    public static bool IsUnityType(Type type) => (type == typeof(Vector2)) || (type == typeof(Vector3)) ||
    (type == typeof(Rect)) || (type == typeof(Matrix4x4)) || (type == typeof(Color)); // NOTE: What's about scriptable object
    // || (type == typeof(AnimationCurve));
    
    public static bool IsBuiltinRefType(Type type) => (type == typeof(string)); // || type.IsInterface
    // IMPORTANT: I don't want to serialize delegate because it may contains a stack pointer
    // NOTE: Maybe I should only use MulticastDelegate: https://learn.microsoft.com/en-us/archive/blogs/brada/delegate-and-multicastdelegate
    //|| typeof(Delegate).IsAssignableFrom(type); // or type.IsSubclassOf(typeof(Delegate)) || (type == typeof(Delegate))
    
    public static bool IsSerializableType(Type type)
    {
        return type.IsPrimitive || type.IsEnum || IsBuiltinRefType(type) || IsUnityType(type);
    }
    
    /*public static T GetCustomAttribute<T>(this Type type, bool inherit = false) where T : Attribute
    {
        object[] attributes = type.GetCustomAttributes(typeof(T), inherit);
        if (attributes != null && attributes.Length > 0)
            return  attributes[0] as T;
        return null;
    }
    
    public static T GetCustomAttribute<T>(this FieldInfo field, bool inherit = false) where T : Attribute
    {
        object[] attributes = field.GetCustomAttributes(typeof(T), inherit);
        if (attributes != null && attributes.Length > 0)
            return attributes[0] as T;
        return null;
    }*/
    
    public static T GetCustomAttribute<T>(this System.Reflection.ICustomAttributeProvider provider, bool inherit = false) where T : Attribute
    {
        object[] attributes = provider.GetCustomAttributes(typeof(T), inherit);
        if (attributes != null && attributes.Length > 0)
            return (T)attributes[0];
        return null;
    }
    
    public static T GetValueFromField<T>(FieldInfo field, object obj, string name, bool includePrivate = false)
    {
        object fieldValue = field.GetValue(obj);
        
        System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
        if (includePrivate)
            flags |= System.Reflection.BindingFlags.NonPublic;
        FieldInfo resultField = field.FieldType.GetField(name, flags);
        
        return (T)resultField.GetValue(fieldValue);
    }
    
    public static T GetValueFromObject<T>(object obj, string fieldName, bool includePrivate = false)
    {
        System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
        if (includePrivate)
            flags |= System.Reflection.BindingFlags.NonPublic;
        return (T)obj.GetType().GetField(fieldName, flags).GetValue(obj);
    }
    
    public class SerializeData
    {
        public SerializeData parent;
        public object[] objs;
        public FieldInfo field;
        public int depth;
        public int index;
        
        public Type type => field?.FieldType ?? objs[0]?.GetType();
        
        public static SerializeData Create(SerializeData parent, Func<object[]> getObjs, FieldInfo field = null, int index = 0)
        {
            return new SerializeData { parent = parent, objs = getObjs(), field = field, depth = parent.depth + 1, index = index };
        }
    }
    public delegate bool SerializeFilter(SerializeData data);
    public delegate void SerializeFunc(SerializeData data);
    public delegate void SerializeRecursive(SerializeData data, Func<SerializeData> recursive);
    private const int MAX_DEPTH = 128;
    
    public static void SerializeType(object[] objects, SerializeFilter isSerializable, SerializeFunc callback,
                                     SerializeFilter hasSerializable, SerializeRecursive recursiveCallback)
    {
        RecursiveSerializeType(new SerializeData { objs = objects });
        
        // TODO: Test this with an array as the original object
        // TODO: Test this with a serializable as the original object
        void RecursiveSerializeType(SerializeData data)
        {
            if (data.objs == null || data.objs.Length < 1 || data.depth > MAX_DEPTH)
                return;
            
            SerializeFunc recursive = childData => recursiveCallback(data, () => { RecursiveSerializeType(childData); return childData; });
            if (isSerializable(data))
                callback(data);
            else if (hasSerializable(data))
            {
                if (data.type.IsArray)
                {
                    int length = Mathf.Min(Array.ConvertAll(data.objs, array => ((Array)array).Length));
                    for (int i = 0; i < length; i++)
                        recursive(SerializeData.Create(data, () => Array.ConvertAll(data.objs, array => ((Array)array).GetValue(i)), index: i));
                }
                else
                {
                    foreach (FieldInfo field in data.type.GetFields())
                        recursive(SerializeData.Create(data,
                                                       () => Array.ConvertAll(data.objs, obj => obj == null ? null : field.GetValue(obj)), field));
                }
            }
        }
    }
#endregion
    
#region String
    public const int indent = 4;
    
    public static StringBuilder AppendIndentLine(this StringBuilder builder, string str, int indentLevel)
    {
        return builder.AppendIndent(str, indentLevel).Append('\n');
    }
    
    public static StringBuilder AppendIndent(this StringBuilder builder, string str, int indentLevel)
    {
        return builder.Append(' ', indent * indentLevel).Append(str);
    }
    
    public static StringBuilder AppendIndentFormat(this StringBuilder builder, string str, int indentLevel, params object[] args)
    {
        return builder.Append(' ', indent * indentLevel).AppendFormat(str, args);
    }
    
    public static string GetAllString<T>(System.Collections.Generic.IList<T> array, string prefix = "", string postfix = "", int indentLevel = 0, uint lineWidth = 1,
                                         Func<T, int, string> toString = null, StringBuilder builder = null)
    {
        int arrayLength = array?.Count ?? 0;
        int predictLength = arrayLength > 0 ? (array[0]?.ToString().Length ?? 1) : 1;
        builder ??= new StringBuilder(predictLength * arrayLength + prefix.Length + postfix.Length);
        builder.AppendIndent(prefix, indentLevel).Append(arrayLength);
        toString ??= (element, _) => element?.ToString() ?? "[Null]";
        
        for (int i = 0; i < arrayLength; ++i)
        {
            if (i % lineWidth == 0)
                builder.Append('\n').AppendIndent("", indentLevel + 1);
            builder.Append($"[{i}]: {toString(array[i], i)}{(i < arrayLength - 1 ? ", " : "")}");
        }
        builder.Append(postfix);
        return builder.ToString();
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
            path = Path.Combine(dir, $"{fileName}_{i}{fileExt}");
        }
    }
    
    public static string CamelCase(this object o)
    {
        return System.Text.RegularExpressions.Regex.Replace(o.ToString(), "([a-z](?=[A-Z]|[0-9])|[A-Z](?=[A-Z][a-z]|[0-9])|[0-9](?=[^0-9]))", "$1 ");
    }
    
    public static string EscapeString(string str, string ignoreEscape)
    {
        string[] unescapes = new string[]
        {
            "\\", // NOTE: Must be the first element
            "\'", "\"", "\0", "\a", "\b", "\f", "\n", "\r", "\t", "\v",
        };
        string[] escapes = new string[]
        {
            "\\\\",
            "\\\'", "\\\"", "\\0", "\\a", "\\b", "\\f", "\\n", "\\r", "\\t", "\\v",
        };
        
        // NOTE: Create and use StringBuilder.Replace can be slightly faster, but will be slower than String.Replace when you call ToString
        // https://stackoverflow.com/questions/6524528/string-replace-vs-stringbuilder-replace
        for (int i = 0; i < unescapes.Length; ++i)
            if (unescapes[i] != ignoreEscape)
            str = str.Replace(unescapes[i], escapes[i]);
        return str;
    }
#endregion
    
#region Physics
    public static RaycastHit2D BoxCast(Vector2 pos, Vector2 size, Color color)
    {
        GameDebug.DrawBox(pos, size, color);
        return Physics2D.BoxCast(pos, size, 0, Vector2.zero, 0, LayerMask.GetMask("Ground"));
    }
    
    // TODO: Rather than use the sprite extents, we should probably use the collider's size
    public static RaycastHit2D GroundCheck(Vector2 pos, Vector2 spriteExtents, float dirY, Color color)
    {
        // NOTE: The .75f will make sure that if the player falls real close to the wall, nothing will happen
        Vector2 boxSize = new Vector2(spriteExtents.x * .75f, 0.02f);
        Vector2 boxPos = pos + new Vector2(0, spriteExtents.y + boxSize.y / 2) * Mathf.Sign(dirY);
        RaycastHit2D groundInfo = BoxCast(boxPos, boxSize, color);
        if (groundInfo)
            Debug.DrawLine(pos, groundInfo.point, color);
        return groundInfo;
    }
    
    public static Collider2D GetClosestCollider(Vector3 pos, float radius, Collider2D[] colliders, int layerMask)
    {
        int length = Physics2D.OverlapCircleNonAlloc(pos, radius, colliders, layerMask);
        if (length == 0)
            return null;
        
        int closest = 0;
        for (int i = 0; i < length; i++)
            if ((pos - colliders[i].transform.position).sqrMagnitude < (pos - colliders[closest].transform.position).sqrMagnitude)
            closest = i;
        return colliders[closest];
    }
#endregion
    
#region Graphics
    public static void DrawGizmosLine(Vector2 a, Vector2 b, Color color)
    {
        Color temp = Gizmos.color;
        Gizmos.color = color;
        Gizmos.DrawLine(a, b);
        Gizmos.color = temp;
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
    
    private static Material glMat = null;
    
    public static void BeginGL()
    {
        if (!glMat)
        {
            GL.PushMatrix();
            glMat = new Material(Shader.Find("Sprites/Default")) { hideFlags = HideFlags.HideAndDontSave };
            glMat.SetPass(0);
            //GL.LoadOrtho();
        }
        else Debug.LogError("Must call EndGL before calling another BeginGL");
    }
    
    public static void EndGL()
    {
        if (glMat)
        {
            GL.PopMatrix();
            UnityEngine.Object.DestroyImmediate(glMat);
        }
        else Debug.LogError("Must call BeginGL before calling another EndGL");
    }
    
    private static void DrawGL(int mode, Color color, params Vector3[] vertices)
    {
        GL.Begin(mode);
        GL.Color(color);
        foreach (var vertex in vertices)
            GL.Vertex(vertex);
        GL.End();
    }
    
    public static void SetMaterialBlock(SpriteRenderer sr, Action<MaterialPropertyBlock> func)
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        sr.GetPropertyBlock(block);
        func?.Invoke(block);
        sr.SetPropertyBlock(block);
    }
#endregion
    
#region GameObject
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
            UnityEngine.Object.Destroy(child.gameObject);
    }
    
    public static void DeactiveGameObject(this MonoBehaviour behaviour, float time)
    {
        behaviour.InvokeAfter(time, () => behaviour.gameObject.SetActive(false));
    }
    
    // NOTE: Use this to destroy in the OnValidate or in the editor
    public static IEnumerator DestroyInEditor(UnityEngine.Object go)
    {
        yield return new WaitForEndOfFrame();
        UnityEngine.Object.DestroyImmediate(go);
    }
#endregion
    
#region Coroutine
    public static IEnumerator InvokeAfter(float delayTime, Action action, Func<bool> waitingCondition)
    {
        float duration = delayTime + Time.time;
        while (Time.time <= duration && waitingCondition())
            yield return null;
        action();
    }
    
    public static Coroutine InvokeAfter(this MonoBehaviour behaviour, float delayTime, Action action, bool useRealTime = false)
    {
        if (behaviour != null && action != null)
            return behaviour.StartCoroutine(InvokeAfter());
        return null;
        
        IEnumerator InvokeAfter()
        {
            if (delayTime > 0)
            {
                if (useRealTime)
                    yield return new WaitForSecondsRealtime(delayTime);
                else
                    yield return new WaitForSeconds(delayTime);
            }
            action();
        }
    }
    
    public static Coroutine InvokeAfter(this MonoBehaviour behaviour, float delayTime, Action action, Func<bool> waitingCondition)
    {
        if (behaviour != null && action != null)
            return behaviour.StartCoroutine(InvokeAfter(delayTime, action, waitingCondition));
        return null;
    }
    
    public static Coroutine InvokeAfterFrames(this MonoBehaviour behaviour, int frame, Action action)
    {
        if (behaviour != null && action != null)
            return behaviour.StartCoroutine(WaitFrames());
        return null;
        
        IEnumerator WaitFrames()
        {
            if (frame == -1)
                yield return new WaitForEndOfFrame();
            else
                for (int i = 0; i < frame; i++)
                yield return null;
            action();
        }
    }
    
    public static Coroutine EmptyCoroutine(this MonoBehaviour mb)
    {
        return mb.StartCoroutine(Empty());
        static IEnumerator Empty() { yield break; }
    }
#endregion
    
#region Time
    // TODO: Only stop time on a global gameObject that can't be destroyed.
    public static IEnumerator StopTime(float time)
    {
        if (time <= 0)
            yield break;
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(time);
        Time.timeScale = 1;
    }
    
    public static IEnumerator SlowdownTime(float duration, float timeScale = .1f)
    {
        float defTimeScale = Time.timeScale;
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = defTimeScale;
    }
#endregion
    
#region Transform
    public static Vector2 Direction(this Transform transform)
    {
        return transform.up + transform.right;
    }
    
    public static Vector2 GetDirectionalPos(Transform target, Vector2 offset)
    {
        return (Vector2)target.position + offset * new Vector2(target.right.x, target.up.y);
    }
    
    public static Vector2 GetDirectionalPos(Transform target, Vector2 offset, float upVector)
    {
        return (Vector2)target.position + offset * new Vector2(target.right.x, upVector);
    }
#endregion
    
#region Misc
    public static Vector2 HalfSize(this Camera camera)
    {
        return new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize);
    }
    
    public static void Play(this AudioSource source, AudioClip clip, float volume)
    {
        source.clip = clip;
        source.volume = volume;
        source.Play();
    }
#endregion
}
