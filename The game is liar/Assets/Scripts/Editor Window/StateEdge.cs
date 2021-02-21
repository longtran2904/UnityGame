using UnityEditor;
using UnityEngine;

[System.Serializable]
public class StateEdge
{
    public StateNode fromNode;
    public StateNode toNode;
    public CustomLine line;

    public StateEdge(StateNode from)
    {
        fromNode = from;
        toNode = null;
    }

    public bool Compare(StateEdge edge)
    {
        return edge.fromNode == fromNode && edge.toNode == toNode;
    }

    public CustomEvent HandleEvent(Event e)
    {
        switch (e.type)
        {
            case EventType.ContextClick:
                if (MathUtils.DistanceLineSegmentPoint(e.mousePosition, line.start, line.end) <= line.size)
                    return CustomEvent.ContextMenu;
                break;
        }
        return CustomEvent.None;
    }

    public void Paint()
    {
        if (fromNode == null || toNode == null)
            return;
        Handles.color = Color.white;
        Handles.DrawAAPolyLine(10, fromNode.box.center, toNode.box.center);
        line = new CustomLine(fromNode.box.center, toNode.box.center, 10);
    }

    public void Paint(Vector2 mousePos)
    {
        if (fromNode == null)
            return;
        Handles.color = Color.white;
        Handles.DrawAAPolyLine(5, fromNode.box.center, mousePos);
    }
}

public struct CustomLine
{
    public Vector2 start;
    public Vector2 end;
    public float size;
    public float length
    {
        get
        {
            return Vector2.Distance(start, end);
        }
    }
    public Vector2 center
    {
        get
        {
            return (start - end) / 2;
        }
    }

    public CustomLine(Vector2 start, Vector2 end, int size = 0)
    {
        this.start = start;
        this.end = end;
        this.size = size;
    }
}
