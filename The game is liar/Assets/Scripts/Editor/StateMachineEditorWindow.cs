using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
 * NOTE:
 * 1. EventType.ContextClick: Right-click somewhere -> EventType.MouseDown -> Release click -> EventType.MouseUp -> EventType.ContextClick
 * 2. EventType.DragPerform: Drag something in -> EventType.DragUpdated -> -> Release drag -> [EventType.DragPerform only when done something
 * with DragAndDrop in EventType.DragUpdated (e.g DragAndDrop.visualMode = ...)] -> EventType.DragExited
 * 3. EventType.current.Use(): set the Event.current.type to Used for the rest of the frame, reset when enter next frame.
 * 4. GUI uses Rect which uses a different coordinate system: (0, 0) on the top left conner
 * 
 * TODO:
 * 1. Snap To Grid
 * 2. Pan and zoom the window around
 * 3. Fix the close selection window change the Event.delta
 */
public class StateMachineEditorWindow : EditorWindow
{
    private StateGraph graph;
    private StateNode connectionStartNode;
    private EditorZoom zoomer = new EditorZoom();
    private int graphPickerWindow;

    private Vector2 nodeSize = new Vector2(240, 80);
    private int gridSize = 8;
    private bool snapToGrid;

    [MenuItem("Window/State Machine Editor")]
    public static StateMachineEditorWindow OpenStateMachineEditorWindow()
    {
        return GetWindow<StateMachineEditorWindow>("State Machine");
    }

    [UnityEditor.Callbacks.OnOpenAsset(1)]
    public static bool OnOpenDatabase(int instanceID, int line)
    {
        StateGraph graph = EditorUtility.InstanceIDToObject(instanceID) as StateGraph;
        if (graph)
        {
            StateMachineEditorWindow window = OpenStateMachineEditorWindow();
            window.Init(graph);
            return true;
        }
        return false;
    }

    private void Init(StateGraph graph)
    {
        this.graph = graph;
        Vector2 nodePos = position.size / 2 - nodeSize / 2;
        if (graph.anyNode.state == null)
        {
            EnemyState state = CreateInstance<EnemyState>();
            state.name = "Any State";
            state.Name = "Any State";
            graph.anyNode = new StateNode(nodePos, nodeSize, state);
            graph.nodes.Add(graph.anyNode);
            AssetDatabase.AddObjectToAsset(state, graph);
        }
        if (graph.startNode.state == null)
        {
            EnemyState state = CreateInstance<EnemyState>();
            state.name = "Start State";
            state.Name = "Start State";
            graph.startNode = new StateNode(nodePos + new Vector2(0, nodeSize.y * 2), nodeSize, state);
            graph.nodes.Add(graph.startNode);
            AssetDatabase.AddObjectToAsset(state, graph);
        }
    }

    private void OnGUI()
    {
        // TODO: Optimize -> Only call Repaint and other Paint method when need
        //DrawGrid(gridSize, new Color(0, 0, 0, .3f));
        //DrawGrid(gridSize * 10, new Color(0, 0, 0, .5f));
        PaintButtons();
        if (graph)
        {
            HandleEvents(Event.current);
            zoomer.Begin();
            PaintEdges();
            PaintNodes();
            zoomer.End();
        }
        Repaint();
    }
    
    private bool HandleEvents(Event e)
    {
        // Handle node's events

        foreach (var node in graph.nodes)
        {
            CustomEvent nodeEvent = node.HandleEvent(e);

            switch (nodeEvent)
            {
                case CustomEvent.ContextMenu:
                    OpenContextMenuForNode(node);
                    break;
                case CustomEvent.Select:
                    if (connectionStartNode != null && node != connectionStartNode)
                    {
                        connectionStartNode.state.transitions.Add(new EnemyTransition(node.state));
                        connectionStartNode = null;
                    }
                    else
                    {
                        if (snapToGrid) node.box.position = MathUtils.Round(node.box.position, gridSize);
                        Selection.activeObject = node.state;
                    }
                    break;
            }

            if (node.state.name != node.state.Name)
            {
                node.state.name = node.state.Name;
                //AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(node.state), node.state.Name);
            }

            if (nodeEvent != CustomEvent.None)
            {
                return true;
            }
        }

        // Handle window's event
        switch (e.type)
        {
            case EventType.ContextClick:
                OpenContextMenu(e.mousePosition);
                return true;
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    connectionStartNode = null;
                    Selection.activeObject = graph;
                    return true;
                }
                break;
            case EventType.KeyDown:
                if (e.keyCode == KeyCode.Escape)
                {
                    connectionStartNode = null;
                    return true;
                }
                break;
        }

        return false;
    }
    
    private void OpenContextMenu(Vector2 mousePos)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Create New State"), false, () =>
        {
            EnemyState state = CreateInstance<EnemyState>();
            state.Name = CreateUniqueName();
            state.name = state.Name;
            StateNode node = new StateNode(mousePos, nodeSize, state);
            graph.nodes.Add(node);
            AssetDatabase.AddObjectToAsset(node.state, graph);
            AssetDatabase.SaveAssets();
        });
        menu.ShowAsContext();
    }

    private string CreateUniqueName()
    {
        string name = "New State";
        int i = 1;
        bool isDone = true;
        while (isDone)
        {
            isDone = false;
            foreach (var n in graph.nodes)
            {
                if (n.state.name == name)
                {
                    name = "New State " + i.ToString();
                    i++;
                    isDone = true;
                    break;
                }
            }
        }
        return name;
    }

    private void OpenContextMenuForNode(StateNode node)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Create New Transition"), false, () =>
        {
            connectionStartNode = node;
        });
        menu.AddItem(new GUIContent("Delete State"), false, () =>
        {
            graph.nodes.Remove(node);
            graph.ClearCache();
            AssetDatabase.RemoveObjectFromAsset(node.state);
            AssetDatabase.SaveAssets();

            // Remove all transitions that transit to destroyed node
            foreach (var n in graph.nodes)
                for (int i = n.state.transitions.Count - 1; i >= 0; i--)
                    if (n.state.transitions[i].nextState == node.state)
                        n.state.transitions.RemoveAt(i);
        });
        menu.ShowAsContext();
    }

    private void PaintButtons()
    {
        GUILayout.BeginArea(new Rect(0, 0, position.width, 20), EditorStyles.toolbar);
        GUILayout.BeginHorizontal();

        if (graph != null)
            GUILayout.Label($"Selected graph: {graph.name}");
        else
            GUILayout.Label($"No graph selected");

        snapToGrid = GUILayout.Toggle(snapToGrid, "Snap to grid", GUILayout.Width(120));

        if (GUILayout.Button(new GUIContent("Select in inspector"), EditorStyles.toolbarButton, GUILayout.Width(150)))
            if (graph != null)
                Selection.activeObject = graph;

        if (GUILayout.Button(new GUIContent("Select level graph"), EditorStyles.toolbarButton, GUILayout.Width(150)))
        {
            graphPickerWindow = GUIUtility.GetControlID(FocusType.Passive) + 100;
            EditorGUIUtility.ShowObjectPicker<StateGraph>(null, false, string.Empty, graphPickerWindow);
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        // Do this here because Clear Window need to call at the end of frame
        if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == graphPickerWindow)
        {
            graphPickerWindow = -1;
            var pickGraph = EditorGUIUtility.GetObjectPickerObject() as StateGraph;

            if (pickGraph != null)
                graph = pickGraph;
            else
            {
                //Clear Window
                graph.ClearCache();
                graph = null;
                connectionStartNode = null;
            }
        }
    }

    private void PaintEdges()
    {
        if (connectionStartNode != null)
            Handles.DrawAAPolyLine(5, connectionStartNode.box.center, Event.current.mousePosition);

        List<CustomLine> edges = new List<CustomLine>();
        foreach (var node in graph.nodes)
        {
            foreach (var transition in node.state.transitions)
            {
                if (transition.nextState)
                {
                    Vector2 startPos = node.box.center;
                    Vector2 endPos = graph.GetNodeByState(transition.nextState).box.center;

                    CustomLine edge = new CustomLine(startPos, endPos, 10);
                    if (edges.Contains(edge))
                        continue; // TODO: Draw multiple arrow like in the animator window when you have multiple transitions transit to the same state

                    foreach (CustomLine e in edges)
                    {
                        if (e.Overlaps(edge))
                        {
                            edge = new CustomLine(startPos += Vector2.one * 10, endPos += Vector2.one * 10, 10);
                            break;
                        }
                    }
                    edges.Add(edge);

                    DrawArrow(startPos, endPos);
                    Handles.DrawAAPolyLine(10, startPos, endPos); // Draw the edge between nodes
                }
            }
        }
    }

    private void DrawArrow(Vector2 startPos, Vector2 endPos)
    {
        Vector2 center = MathUtils.Average(startPos, endPos);
        Color color = Handles.color;
        Vector2 dir = (endPos - startPos).normalized;

        // The 3 points of the equilateral triangle
        Vector3 a = center + dir * 10;
        Vector3 b = center + MathUtils.Rotate(dir, 120 * Mathf.Deg2Rad) * 10;
        Vector3 c = center + MathUtils.Rotate(dir, -120 * Mathf.Deg2Rad) * 10;

        // Change the point's coordinate from GUI's Coordinate ((0, 0) on the top left conner) to the normal coordinate ((0, 0) on the bottom left conner)
#if true
        a.y = position.height - 13.5f - a.y;
        b.y = position.height - 13.5f - b.y;
        c.y = position.height - 13.5f - c.y;
#else
        a.y = position.height - a.y;
        b.y = position.height - b.y;
        c.y = position.height - c.y;
#endif

        // Return the value in [(0, 0), (1, 1)] because that what GL.Vertex use
        a = MathUtils.InverseLerp(Vector2.zero, position.size, a);
        b = MathUtils.InverseLerp(Vector2.zero, position.size, b);
        c = MathUtils.InverseLerp(Vector2.zero, position.size, c);

        GameUtils.DrawGLTriangle(a, b, c);
    }

    private void PaintNodes()
    {
        foreach (var node in graph.nodes)
        {
            //node.box.position += zoomer.GetDelta();
            node.Paint();
        }
    }

    private void DrawGrid(float gridSpacing, Color gridColor)
    {
        gridSpacing = gridSpacing * zoomer.zoom;

        var widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        var heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        var originalHandleColor = Handles.color;
        Handles.color = gridColor;

        //Vector3 panOffset = zoomer.GetContentOffset() + position.size / 2;
        //Vector3 panOffset += -(zoomer.zoom * (zoomCenter - panOffset * oldZoom) - zoomCenter * oldZoom) / (zoomer.zoom * oldZoom) - panOffset;
        //Vector3 newOffset = new Vector3((zoomer.zoom * panOffset.x) % gridSpacing, (zoomer.zoom * panOffset.y) % gridSpacing, 0);
        Vector3 newOffset = Vector3.zero;
        //Vector3 newOffset = panOffset;
        //InternalDebug.Log(newOffset);

        // Draw vertical lines
        for (var i = 0; i < widthDivs + 1; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height + gridSpacing, 0f) + newOffset);
        }

        // Draw horizontal lines
        for (var j = 0; j < heightDivs + 1; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width + gridSpacing, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = originalHandleColor;
        Handles.EndGUI();
    }
}