using System.IO;
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
 * 1. Fix triangle arrow
 * 3. Snap To Grid
 * 4. Pan and zoom the window around
 * 5. Fix the close selection window change the Event.delta
 */
public class StateMachineEditorWindow : EditorWindow
{
    private StateGraph graph;
    private StateNode connectionStartNode;
    private EditorZoom zoomer = new EditorZoom();

    private int statePickerWindow;
    private int graphPickerWindow;

    private Vector2 nodeSize = new Vector2(250, 50);
    private Vector2 mousePosition; // Save the mouse position when you open the selection window
    private float gridSize = 8;
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
            window.graph = graph;
            return true;
        }
        return false;
    }

    [MenuItem("Assets/Create/Graph From Selected States")]
    static void CreateGraphFromStates()
    {
        if (Selection.objects.Length > 1)
        {
            StateMachineEditorWindow window = OpenStateMachineEditorWindow();

            StateGraph graph = CreateInstance<StateGraph>();
            Vector2 offset = new Vector2(50, 50);
            if (!window.CreateNodesFromPosition(Selection.objects, graph, offset, offset))
            {
                DestroyImmediate(graph);
                return;
            }

            string pathToSelection = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.objects[0]));
            GameUtils.CreateAssetFile(graph, GameUtils.CreateUniquePath(pathToSelection + "/New Graph.asset"), true);
            graph.pathToSaveNewState = pathToSelection;
            window.graph = graph;
        }
    }

    private void OnGUI()
    {
        // TODO: Optimize -> Only call Repaint and other Paint method when need
        DrawGrid(gridSize, new Color(0, 0, 0, .3f));
        DrawGrid(gridSize * 10, new Color(0, 0, 0, .5f));
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
                        Selection.activeObject = node.state ?? node.anyState;
                    break;
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
            case EventType.DragUpdated:
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                Event.current.Use();
                break;
            case EventType.DragPerform:
                DragAndDrop.AcceptDrag();
                // Check if is dragged from Asset Folder
                if (DragAndDrop.paths.Length == DragAndDrop.objectReferences.Length)
                    CreateNodesFromPosition(DragAndDrop.objectReferences, graph, e.mousePosition, new Vector2(50, 50));
                break;
        }

        if (e.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == statePickerWindow)
        {
            statePickerWindow = -1;
            EnemyState state = EditorGUIUtility.GetObjectPickerObject() as EnemyState;
            AnyState anyState = EditorGUIUtility.GetObjectPickerObject() as AnyState;
            if (state && !graph.nodes.Contains(graph.GetNodeByState(state)))
            {
                graph.nodes.Add(new StateNode(mousePosition, nodeSize, state));
                return true;
            }
            else if (anyState && !graph.anyState)
            {
                graph.nodes.Add(new StateNode(mousePosition, nodeSize, anyState));
                return true;
            }
        }

        return false;
    }

    private bool CreateNodesFromPosition(Object[] objects, StateGraph graph, Vector2 startPos, Vector2 distanceBtwNode)
    {
        Vector2 pos = startPos;
        foreach (Object obj in objects)
        {
            EnemyState state = obj as EnemyState;
            if (state)
                AddNode(new StateNode(pos, nodeSize, state));
            else
            {
                AnyState any = obj as AnyState;
                if (any && !graph.anyState)
                {
                    AddNode(new StateNode(pos, nodeSize, any));
                    graph.anyState = any;
                }
                else
                    return false;
            }
        }
        return true;

        void AddNode(StateNode node)
        {
            InternalDebug.Log(node.state ?? node.anyState);
            graph.nodes.Add(node);
            pos.x += nodeSize.x + distanceBtwNode.x;
            if (pos.x + nodeSize.x >= position.width)
                pos = new Vector2(startPos.x, pos.y + nodeSize.y + distanceBtwNode.y);
        }
    }
    
    private void OpenContextMenu(Vector2 mousePos)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Create New State"), false, () =>
        {
            StateNode node = new StateNode(mousePos, nodeSize, CreateInstance<EnemyState>(), CreateUniqueName());
            graph.nodes.Add(node);
            graph.temporaryState.Add(node.state);
        });
        menu.AddItem(new GUIContent("Create Any State"), false, () =>
        {
            if (!graph.anyState)
            {
                StateNode node = new StateNode(mousePos, nodeSize, CreateInstance<AnyState>());
                graph.nodes.Add(node);
                graph.anyState = node.anyState; 
            }
        });
        menu.AddItem(new GUIContent("Add Existing State"), false, () =>
        {
            mousePosition = mousePos;
            statePickerWindow = EditorGUIUtility.GetControlID(FocusType.Passive) + 100;
            EditorGUIUtility.ShowObjectPicker<EnemyState>(null, false, string.Empty, statePickerWindow);
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

            // Remove all transitions that transit to destroyed node
            foreach (var n in graph.nodes)
                for (int i = n.state.transitions.Count - 1; i >= 0; i--)
                    if (n.state.transitions[i].nextState == node.state)
                        n.state.transitions.RemoveAt(i);

            if (graph.temporaryState.Contains(node.state))
            {
                DestroyImmediate(node.state);
                graph.temporaryState.Remove(node.state);
            }
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

        if (GUILayout.Button(new GUIContent("Export All New States"), EditorStyles.toolbarButton, GUILayout.Width(150)))
        {
            foreach (var state in graph.temporaryState)
            {
                GameUtils.CreateAssetFile(state, graph.pathToSaveNewState + $"/{state.Name}.asset");
            }
            graph.temporaryState.Clear();
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
            AnyState state = node.state ?? node.anyState;
            if (!state) continue;
            foreach (var transition in state.transitions)
            {
                if (transition.nextState)
                {
                    StateNode endNode = graph.GetNodeByState(transition.nextState);
                    if (endNode == null) // When add an existing state that has a transition to a outside state that isn't in graph
                        continue;

                    Vector2 startPos = node.box.center;
                    Vector2 endPos = endNode.box.center;

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
        Vector2 dir = (endPos - startPos).normalized;

        // The 3 points of the equilateral triangle
        Vector3 a = center + dir * 10;
        Vector3 b = center + MathUtils.Rotate(dir, 120 * Mathf.Deg2Rad) * 10;
        Vector3 c = center + MathUtils.Rotate(dir, -120 * Mathf.Deg2Rad) * 10;

        // Change the point's coordinate from GUI's Coordinate ((0, 0) on the top left conner) to the normal coordinate ((0, 0) on the bottom left conner)
        a.y = position.height - 1 - a.y;
        b.y = position.height - 1 - b.y;
        c.y = position.height - 1 - c.y;

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
            node.Paint();
        }
    }

    private void DrawGrid(float gridSpacing, Color gridColor)
    {
        var widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        var heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        var originalHandleColor = Handles.color;
        Handles.color = gridColor;

        // Draw vertical lines
        for (var i = 0; i < widthDivs + 1; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0), new Vector3(gridSpacing * i, position.height + gridSpacing, 0f));
        }

        // Draw horizontal lines
        for (var j = 0; j < heightDivs + 1; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0), new Vector3(position.width + gridSpacing, gridSpacing * j, 0f));
        }

        Handles.color = originalHandleColor;
        Handles.EndGUI();
    }
}
