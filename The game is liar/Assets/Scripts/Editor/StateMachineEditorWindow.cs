using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class StateMachineEditorWindow : EditorWindow
{
    private Vector2 nodeSize = new Vector2(250, 50);
    private StateGraph graph;
    private StateEdge pendingConnection;

    [MenuItem("Window/State Machine Editor")]
    public static StateMachineEditorWindow OpenStateMachineEditorWindow()
    {
        return GetWindow<StateMachineEditorWindow>("State Machine");
    }

    [UnityEditor.Callbacks.OnOpenAsset(1)]
    public static bool OnOpenDatabase(int instanceID, int line)
    {
        StateGraph _graph = EditorUtility.InstanceIDToObject(instanceID) as StateGraph;
        if (_graph)
        {
            StateMachineEditorWindow window = OpenStateMachineEditorWindow();
            window.graph = _graph;
            window.CreateNodesFromStates();
            return true;
        }
        return false;
    }

    private void OnGUI()
    {
        if (graph)
        {
            HandleEvents(Event.current);
            PaintEdges();
            PaintNodes();
            Repaint();
        }
    }

    // NOTE: For Debugging -> Clear all invalid edges
    private void RemoveInValidEdges()
    {
        for (int i = graph.edges.Count - 1; i >= 0; i--)
            if (!graph.nodes.Contains(graph.edges[i].fromNode) || !graph.nodes.Contains(graph.edges[i].toNode))
                graph.edges.Remove(graph.edges[i]);
    }

    private bool HandleEvents(Event e)
    {
        foreach (var node in graph.nodes)
        {
            CustomEvent nodeEvent = node.HandleEvent(e);

            switch (nodeEvent)
            {
                case CustomEvent.ContextMenu:
                    OpenContextMenuForNode(node, e.mousePosition);
                    break;
                case CustomEvent.Select:
                    if (pendingConnection != null && node != pendingConnection.fromNode)
                        CreatePendingConnection(node);
                    else
                    {
                        InternalDebug.Log("Select");
                        //Selection.activeObject = node.state;
                    }
                    break;
            }

            if (nodeEvent != CustomEvent.None)
            {
                return true;
            }
        }

        foreach (var edge in graph.edges)
        {
            CustomEvent edgeEvent = edge.HandleEvent(e);

            switch (edgeEvent)
            {
                case CustomEvent.ContextMenu:
                    OpenContextMenuForEdge(edge);
                    break;
            }

            if (edgeEvent != CustomEvent.None)
            {
                InternalDebug.Log("Edge Event: " + edgeEvent);
                return true;
            }
        }

        switch (e.type)
        {
            case EventType.ContextClick:
                OpenContextMenu(e.mousePosition);
                return true;
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    DeletePendingConnection();
                    return true;
                }
                break;
            case EventType.KeyDown:
                if (e.keyCode == KeyCode.Escape)
                {
                    DeletePendingConnection();
                    return true;
                }
                break;
        }

        return false;
    }

    private void OpenContextMenu(Vector2 mousePos)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Add New State"), false, () =>
        {
            StateNode node = new StateNode(mousePos, new Vector2(200, 50), CreateInstance<EnemyState>());
            graph.nodes.Add(node);
        });
        menu.ShowAsContext();
    }

    private void OpenContextMenuForNode(StateNode node, Vector2 mousePos)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Create New Transition"), false, () =>
        {
            pendingConnection = new StateEdge(node);
        });
        menu.AddItem(new GUIContent("Delete State"), false, () =>
        {
            graph.nodes.Remove(node);
            RemoveEdgesFromNode(node);
        });
        menu.ShowAsContext();
    }

    private void RemoveEdgesFromNode(StateNode node)
    {
        for (int i = graph.edges.Count - 1; i >= 0; i--)
            if (graph.edges[i].fromNode == node || graph.edges[i].toNode == node)
                graph.edges.Remove(graph.edges[i]);
    }

    private void OpenContextMenuForEdge(StateEdge edge)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Delete Edge"), false, () =>
        {
            graph.edges.Remove(edge);
        });
        menu.ShowAsContext();
    }

    private void CreatePendingConnection(StateNode node)
    {
        pendingConnection.toNode = node;

        // Make sure the pending connecction doesn't get duplicate
        foreach (var edge in graph.edges)
        {
            if (pendingConnection.Compare(edge))
            {
                pendingConnection.toNode = null;
                return;
            }
        }

        graph.edges.Add(pendingConnection);
        pendingConnection = null;
    }

    private void DeletePendingConnection()
    {
        if (pendingConnection != null)
            pendingConnection = null;
    }

    private void CreateNodesFromStates()
    {
        Vector2 offset = new Vector2(50, 50);
        Vector2 pos = offset;
        foreach (var state in graph.states)
        {
            graph.nodes.Add(new StateNode(pos, nodeSize, state));
            pos.x += offset.x + nodeSize.x;
            if (pos.x >= position.width)
            {
                pos.x = offset.x;
                pos.y += offset.y + nodeSize.y;
            }
        }
    }

    private void PaintNodes()
    {
        foreach (var node in graph.nodes)
        {
            node.Paint();
        }
    }

    private void PaintEdges()
    {
        if (pendingConnection != null)
            pendingConnection.Paint(Event.current.mousePosition);

        foreach (var edge in graph.edges)
        {
            edge.Paint();
        }
    }
}
