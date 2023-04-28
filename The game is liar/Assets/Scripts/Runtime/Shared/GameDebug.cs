using System.Diagnostics;
using System;
using UnityEngine;
using System.Collections.Generic;

public enum CaptureType
{
    None,
    True,
    False,
    Changed,
}

public class DebugInput
{
    public DebugInput next;
    public InputType trigger, increase, decrease;
    public float value;
    public RangedFloat range;
    public Func<float, int, float> updateValue;
    public Action<float> changed;
    public Action<float> callback;
}

// RANT: Unity doesn't provide a way to manipulate or at least ignore a function in its debug's call stack
// so the only thing that you can do is to make it into a separate DLL.
public static class GameDebug
{
    private class DebugData
    {
        public bool disableLog;
        public bool disableBreak;
        public string name;
        public Action<bool> assert;
        public DiagramEntry firstDiagram;
        public DiagramEntry lastDiagram;
        public CaptureEntry firstCapture;
        public CaptureEntry lastCapture;
        public DebugInput firstInput;
    }

    private enum DiagramType
    {
        None,

        Circle,
        Line,
        Box,
        Overlay,

        Count
    }

    private class DiagramEntry
    {
        public DiagramEntry next;
        public DiagramType type;
        public int id;
        public Vector2 p1;
        public Vector2 p2;
        public Color color;
    }

    private class CaptureEntry
    {
        public CaptureEntry next;
        public int id;
        public bool prevCapture;
        public int runCount;
        public int captureCount;
    }

    public static bool logEnabled { get => UnityEngine.Debug.unityLogger.logEnabled; set => UnityEngine.Debug.unityLogger.logEnabled = value; }
    private static List<DebugData> data = new List<DebugData>(8) { new DebugData() { name = "Default" } };
    private static int current;

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void Log(object message, bool debugBreak = false, LogType logType = LogType.Log)
    {
        DebugData currentData = data[current];
        bool isLogImportant = logType != LogType.Log && logType != LogType.Warning;
        if (isLogImportant)
            currentData.assert?.Invoke(false);

        if (!currentData.disableLog || isLogImportant)
            UnityEngine.Debug.LogFormat(logType, LogOption.None, null, message?.ToString() ?? "Null", new object[0]);

        if (debugBreak)
            Break();
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void Assert(bool condition, object message, bool debugBreak = false)
    {
        if (!condition)
            Log(message, debugBreak, LogType.Assert);
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void Assert(bool condition, object message, Action success, bool debugBreak = false)
    {
        Assert(condition, message, debugBreak);
        data[current].assert?.Invoke(condition);
        success?.Invoke();
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void Break()
    {
        DebugData currentData = data[current];
        if (!currentData.disableBreak)
            UnityEngine.Debug.Break();
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void ToggleLogger()
    {
        bool enable = logEnabled;
        if (enable)
        {
            Log("Enable Log: " + !enable);
            logEnabled = !enable;
        }
        else
        {
            logEnabled = !enable;
            Log("Enable Log: " + !enable);
        }
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    // RANT: Unity doesn't have any API to clear the debug console.
    // You need to do some stupid shit like introspect some DLLs and call some method from it.
    // This is not an official way, and Unity has broken it by changing the DLL and method's name from time to time.
    public static void ClearLog()
    {
        var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    public static void Capture(int id, CaptureType type, bool canCapture, int startCaptureIndex = 0, int maxCaptureCount = 0, Action callback = null)
    {
        CaptureEntry capture = null;
        {
            for (CaptureEntry entry = data[current].firstCapture; entry != null; entry = entry.next)
            {
                if (entry.id == id)
                {
                    capture = entry;
                    break;
                }
            }

            if (capture == null)
            {
                capture = new CaptureEntry { id = id };
                DebugData current = data[GameDebug.current];
                if (current.lastCapture == null)
                    current.lastCapture = current.firstCapture = capture;
                else
                    current.lastCapture = current.lastCapture.next = capture;
            }
        }

        if (capture.runCount++ < startCaptureIndex)
            return;
        if (maxCaptureCount != 0 && capture.captureCount > maxCaptureCount)
            return;

        bool success = false;
        switch (type)
        {
            case CaptureType.True:    { success =  canCapture;                        } break;
            case CaptureType.False:   { success = !canCapture;                        } break;
            case CaptureType.Changed: { success =  canCapture != capture.prevCapture; } break;
        }

        capture.prevCapture = canCapture;
        if (success)
        {
            capture.captureCount++;
            callback?.Invoke();
        }
    }

    public static void BindInput(DebugInput input)
    {
        input.next = data[current].firstInput;
        data[current].firstInput = input;
    }

    public static void BindInput(InputType executeType, Action executeFunc)
    {
        BindInput(new DebugInput { trigger = executeType, callback = _ => executeFunc() });
    }

    public static void UpdateInput()
    {
        foreach (DebugData debug in data)
        {
            for (DebugInput input = debug.firstInput; input != null; input = input.next)
            {
                int delta = 0;
                if (GameInput.GetInput(input.increase)) delta += 1;
                if (GameInput.GetInput(input.decrease)) delta -= 1;

                if (input.updateValue != null)
                {
                    float value = input.updateValue(input.value, delta);
                    if (input.range.range != 0)
                        value = Mathf.Clamp(value, input.range.min, input.range.max);
                    if (input.value != value)
                        input.changed?.Invoke(value);
                    input.value = value;
                }

                if (GameInput.GetInput(input.trigger))
                    input.callback(input.value);
            }
        }
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void BeginDebug(string name, bool enableLog, bool enableBreak, Action<bool> assert = null)
    {
        int index = FindDataIndex(name);
        if (index < 0)
        {
            data.Add(new DebugData { name = name, disableLog = !enableLog, disableBreak = !enableBreak, assert = assert });
            index = data.Count - 1;
        }
        current = index;
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void EndDebug()
    {
        if (current > 0)
            current--;
        // else TODO: Logging()
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void DrawBox(Vector2 center, Vector2 size, Color color, bool gizmos = false)
    {
        Vector2 extents = size / 2;
        Vector2 topLeft = center + new Vector2(-extents.x, extents.y);
        Vector2 topRight = center + extents;
        Vector2 botLeft = center - extents;
        Vector2 botRight = center + new Vector2(extents.x, -extents.y);

        DrawLine(topLeft , topRight, color, gizmos);
        DrawLine(topLeft , botLeft , color, gizmos);
        DrawLine(botRight, botLeft , color, gizmos);
        DrawLine(botRight, topRight, color, gizmos);
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void DrawBox(Rect rect, Color color, bool gizmos = false)
    {
        DrawBox(rect.center, rect.size, color);
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void DrawBoxMinMax(Vector2 min, Vector2 max, Color color, bool gizmos = false)
    {
        DrawBox((min + max) / 2, max - min, color, gizmos);
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void DiagramBox(Rect rect, Color color, int id = 0)
    {
        AppendDiagramEntry(rect.min, rect.max, DiagramType.Box, color, id);
    }

    public static void DiagramBoxes(List<Rect> list, Color color, int id = 0)
    {
        foreach (Rect rect in list)
            DiagramBox(rect, color, id);
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void DiagramTiles(IList<Vector2> list, Color color, Vector2 tileSize, int id)
    {
        foreach (Vector2 pos in list)
            DiagramBox(MathUtils.CreateRect(pos + new Vector2(.5f, .5f), tileSize), color, id);
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void DiagramConnections(Rect from, IList<Rect> to, Color fromCol, Color toCol, Color conCol, int id, bool drawFrom = true)
    {
        foreach (Rect r in to)
            DiagramConnection(from, r, fromCol, toCol, conCol, id, drawFrom);
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void DiagramConnection(Rect a, Rect b, Color aCol, Color bCol, Color conCol, int id, bool drawA = true, bool drawB = true)
    {
        if (drawA)
            DiagramBox(a, aCol, id);
        if (drawB)
            DiagramBox(b, bCol, id);
        DiagramLine(a.center, b.center, conCol, id);
    }

    public static void DiagramLine(Vector2 a, Vector2 b, Color color, int id)
    {
        AppendDiagramEntry(a, b, DiagramType.Line, color, id);
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    private static void AppendDiagramEntry(Vector2 a, Vector2 b, DiagramType type, Color color, int id)
    {
        DiagramEntry entry = new DiagramEntry { p1 = a, p2 = b, type = type, id = id, color = color };
        DebugData current = data[GameDebug.current];
        if (current.lastDiagram == null)
            current.lastDiagram = current.firstDiagram = entry;
        else
            current.lastDiagram = current.lastDiagram.next = entry;
    }

    private static DebugData FindDebugData(string name)
    {
        int index = FindDataIndex(name);
        return index > 0 ? data[index] : null;
    }

    private static int FindDataIndex(string name)
    {
        for (int i = 0; i < data.Count; i++)
            if (data[i].name == name)
                return i;
        return -1;
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void ClearDiagram(string name)
    {
        DebugData debug = FindDebugData(name);
        if (debug != null)
            debug.firstDiagram = debug.lastDiagram = null;
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void RenderDiagram(string name, bool gizmos, ulong maskID)
    {
        DebugData debug = FindDebugData(name);
        if (debug == null)
        {
            //Log("Can't find any debugger with the name " + name, LogType.Warning);
            return;
        }
        //if (debug.first == null) Log("Debug is empty" + debug.name);

        for (DiagramEntry entry = debug.firstDiagram; entry != null; entry = entry.next)
        {
            if ((maskID & (1ul << entry.id)) != 0)
            {
                switch (entry.type)
                {
                    case DiagramType.Box:
                        {
                            DrawBoxMinMax(entry.p1, entry.p2, entry.color, gizmos);
                        } break;
                    case DiagramType.Line:
                        {
                            DrawLine(entry.p1, entry.p2, entry.color, gizmos);
                        } break;
                }
            }
        }
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void DrawCircle(Vector2 pos, float radius, Color color)
    {
        const int maxPoints = 512;
        float rotPerPoint = 360f / maxPoints;
        for (int i = 0; i < maxPoints; i++)
            DrawLine(pos + MathUtils.MakeVector2(rotPerPoint * i, radius), pos + MathUtils.MakeVector2(rotPerPoint * (i + 1), radius), color);
    }

    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void DrawLine(Vector2 a, Vector2 b, Color color, bool useGizmos = false)
    {
        if (useGizmos)
            GameUtils.DrawGizmosLine(a, b, color);
        else
            UnityEngine.Debug.DrawLine(a, b, color);
    }
}
