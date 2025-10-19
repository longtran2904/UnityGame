//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;

///*
// * NOTE:
// * 1. EventType.ContextClick: Right-click somewhere -> EventType.MouseDown -> Release click -> EventType.MouseUp -> EventType.ContextClick
// * 2. EventType.MouseDrag: EventType.MouseDown -> EventType.MouseDrag -> EventType.MouseUp
// * 2. EventType.DragPerform: Drag something in -> EventType.DragUpdated -> -> Release drag -> [EventType.DragPerform: only when done something
// * with DragAndDrop in EventType.DragUpdated (e.g DragAndDrop.visualMode = ...)] -> EventType.DragExited
// * 3. EventType.current.Use(): set the Event.current.type to Used for the rest of the frame, reset when enter next frame.
// * 4. GUI uses Rect which uses a different coordinate system: (0, 0) on the top left conner
// * 5. Screen.height - position.height = 21
// * 
// * TODO:
// * 1. Snap To Grid
// * 2. Fix the close selection window change the Event.delta
// * 3. Draw Order
// * 4. Fix bug where right-click and then right-click again on a different node
// */
//public class StateMachineEditorWindow : EditorWindow
//{
//    private StateGraph graph;
//    private StateNode connectionStartNode;
//    private StateNode currentSelectNode;
//    private EditorZoom zoomer = new EditorZoom();
//    private int graphPickerWindow;

//    private Vector2 nodeSize = new Vector2(240, 80);
//    private int lineSize = 10;
//    private int gridSize = 8;
//    private bool snapToGrid;

//    [MenuItem("Window/State Machine Editor")]
//    public static StateMachineEditorWindow OpenStateMachineEditorWindow()
//    {
//        return GetWindow<StateMachineEditorWindow>("State Machine");
//    }

//    [UnityEditor.Callbacks.OnOpenAsset(1)]
//    public static bool OnOpenDatabase(int instanceID, int line)
//    {
//        StateGraph graph = EditorUtility.InstanceIDToObject(instanceID) as StateGraph;
//        if (graph)
//        {
//            StateMachineEditorWindow window = OpenStateMachineEditorWindow();
//            window.Init(graph);
//            return true;
//        }
//        return false;
//    }

//    private void Init(StateGraph graph)
//    {
//        this.graph = graph;
//        Vector2 nodePos = position.size / 2 - nodeSize / 2;
//        if (graph.anyNode.state == null)
//        {
//            EnemyState state = CreateInstance<EnemyState>();
//            state.Name = "Any State";
//            graph.anyNode = new StateNode(nodePos, nodeSize, state);
//            graph.nodes.Add(graph.anyNode);
//            AssetDatabase.AddObjectToAsset(state, graph);
//        }
//        if (graph.startNode.state == null)
//        {
//            EnemyState state = CreateInstance<EnemyState>();
//            state.Name = "Start State";
//            graph.startNode = new StateNode(nodePos + new Vector2(0, nodeSize.y * 2), nodeSize, state);
//            graph.nodes.Add(graph.startNode);
//            AssetDatabase.AddObjectToAsset(state, graph);
//        }
//    }

//    private void ResetCurrentNode()
//    {
//        currentSelectNode = null;
//    }

//    private void OnGUI()
//    {
//        PaintButtons();
//        zoomer.Begin();
//        DrawGrid(gridSize, new Color(0, 0, 0, .3f));
//        DrawGrid(gridSize * 10, new Color(0, 0, 0, .5f));
//        if (graph)
//        {
//            HandleEvents();
//            InternalDebug.Log(currentSelectNode?.state.Name);
//            PaintNodes();
//            PaintEdges();
//        }
//        zoomer.End();
//        Repaint();
//    }

//    private void PaintButtons()
//    {
//        GUILayout.BeginArea(new Rect(0, 0, position.width, 20), EditorStyles.toolbar);
//        GUILayout.BeginHorizontal();

//        if (graph != null)
//            GUILayout.Label($"Selected graph: {graph.name}");
//        else
//            GUILayout.Label($"No graph selected");

//        snapToGrid = GUILayout.Toggle(snapToGrid, "Snap to grid", GUILayout.Width(120));

//        if (GUILayout.Button(new GUIContent("Select in inspector"), EditorStyles.toolbarButton, GUILayout.Width(150)))
//            if (graph != null)
//                Selection.activeObject = graph;

//        if (GUILayout.Button(new GUIContent("Select level graph"), EditorStyles.toolbarButton, GUILayout.Width(150)))
//        {
//            graphPickerWindow = GUIUtility.GetControlID(FocusType.Passive) + 100;
//            EditorGUIUtility.ShowObjectPicker<StateGraph>(null, false, string.Empty, graphPickerWindow);
//        }

//        GUILayout.EndHorizontal();
//        GUILayout.EndArea();

//        // Do this here because Clear Window need to call at the end of frame
//        if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == graphPickerWindow)
//        {
//            graphPickerWindow = -1;
//            var pickGraph = EditorGUIUtility.GetObjectPickerObject() as StateGraph;

//            if (pickGraph != null)
//                graph = pickGraph;
//            else
//            {
//                //Clear Window
//                graph.ClearCache();
//                graph = null;
//                connectionStartNode = null;
//            }
//        }
//    }

//    private void HandleEvents()
//    {
//        if (currentSelectNode == null)
//        {
//            int i = 0;
//            foreach (var node in graph.nodes.ToArray())
//            {
//                if (node.box.Contains(zoomer.ConvertScreenPosToRealPos(Event.current.mousePosition)) && Event.current.type == EventType.MouseDown)
//                {
//                    //graph.nodes.ToFirst(i); // put the node at the beginning of the list. Node's order affect the order in which it will drawn and handled event.
//                    currentSelectNode = node;
//                    break;
//                }
//                i++;
//            }
//            // TODO: Change the name of the state object in the assets file 
//        }

//        if (currentSelectNode != null)
//        {
//            if (Event.current.type == EventType.MouseUp)
//                ResetCurrentNode();
//            else
//                HandleNodeEvent();
//        }
        
//        // Handle window's event
//        switch (Event.current.type)
//        {
//            case EventType.MouseDown:
//                if (Event.current.button == 0)
//                {
//                    connectionStartNode = null;
//                    Selection.activeObject = graph;
//                }
//                else if (Event.current.button == 1 && connectionStartNode == null)
//                {
//                    // NOTE: I don't directly use Event.current.mousePosition because CreateNewState will get call outside of OnGUI() which mean event.current is null.
//                    Vector2 mousePos = Event.current.mousePosition;
//                    OpenContextMenu(("Create New State", () => CreateNewState(mousePos)));
//                    Event.current.Use();
//                }
//                break;
//            case EventType.KeyDown:
//                if (Event.current.keyCode == KeyCode.Escape)
//                    connectionStartNode = null;
//                break;
//        }
//    }

//    private void CreateNewState(Vector2 mousePos)
//    {
//        EnemyState state = CreateInstance<EnemyState>();
//        state.Name = CreateUniqueName();
//        StateNode node = new StateNode(mousePos, nodeSize, state);
//        graph.nodes.Add(node);
//        AssetDatabase.AddObjectToAsset(node.state, graph);
//        AssetDatabase.SaveAssets();
//    }

//    private void HandleNodeEvent()
//    {
//        switch (Event.current.type)
//        {
//            case EventType.MouseDown:
//                if (Event.current.button == 0)
//                {
//                    if (connectionStartNode != null && currentSelectNode != connectionStartNode && currentSelectNode != graph.startNode && currentSelectNode != graph.anyNode)
//                    {
//                        connectionStartNode.state.transitions.Add(new EnemyTransition(currentSelectNode.state));
//                        connectionStartNode = null;
//                    }
//                    else
//                        Selection.activeObject = currentSelectNode.state; 
//                }
//                else if (Event.current.button == 1)
//                {
//                    if (currentSelectNode == graph.anyNode || currentSelectNode == graph.startNode)
//                        OpenContextMenu(("Create New Transition", CreateTransition));
//                    else
//                        OpenContextMenu(("Create New Transition", CreateTransition), ("Delete State", DeleteState));
//                }
//                break;
//            case EventType.MouseDrag:
//                currentSelectNode.box.position += Event.current.delta / zoomer.zoom;
//                break;
//            default:
//                return;
//        }
//        Event.current.Use();

//        void CreateTransition()
//        {
//            connectionStartNode = currentSelectNode;
//            ResetCurrentNode();
//        }

//        void DeleteState()
//        {
//            graph.nodes.Remove(currentSelectNode);
//            graph.ClearCache();
//            AssetDatabase.RemoveObjectFromAsset(currentSelectNode.state);
//            AssetDatabase.SaveAssets();

//            // Remove all transitions that transit to destroyed node
//            foreach (var n in graph.nodes)
//                for (int i = n.state.transitions.Count - 1; i >= 0; i--)
//                    if (n.state.transitions[i].nextState == currentSelectNode.state)
//                        n.state.transitions.RemoveAt(i);
//            DestroyImmediate(currentSelectNode.state);
//            ResetCurrentNode();
//        }
//    }

//    private void OpenContextMenu(params (string name, GenericMenu.MenuFunction func)[] items)
//    {
//        GenericMenu menu = new GenericMenu();
//        foreach (var item in items)
//        {
//            menu.AddItem(new GUIContent(item.name), false, item.func);
//        }
//        menu.ShowAsContext();
//    }

//    private string CreateUniqueName()
//    {
//        string name = "New State";
//        int i = 1;
//        bool isDone = true;
//        while (isDone)
//        {
//            isDone = false;
//            foreach (var n in graph.nodes)
//            {
//                if (n.state.Name == name)
//                {
//                    name = "New State " + i.ToString();
//                    i++;
//                    isDone = true;
//                    break;
//                }
//            }
//        }
//        return name;
//    }

//    private void DrawGrid(float gridSpacing, Color gridColor)
//    {
//        var widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
//        var heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

//        var originalHandleColor = Handles.color;
//        Handles.color = gridColor;

//        Vector3 newOffset = zoomer.zoomOrigin;
//        int scale = 200; // If scale = 0 then it will only draw the grid to fit the current screen
//        float start = gridSpacing + gridSpacing * scale;

//        // Draw vertical lines
//        for (var i = -scale; i < widthDivs + scale; i++)
//        {
//            Handles.DrawLine(new Vector3(gridSpacing * i, -start, 0) + newOffset, new Vector3(gridSpacing * i, position.height + start, 0f) + newOffset);
//        }

//        // Draw horizontal lines
//        for (var j = -scale; j < heightDivs + scale; j++)
//        {
//            Handles.DrawLine(new Vector3(-start, gridSpacing * j, 0) + newOffset, new Vector3(position.width + start, gridSpacing * j, 0f) + newOffset);
//        }

//        Handles.color = originalHandleColor;
//    }

//    private void PaintNodes()
//    {
//        Color boxColor = new Color(.3f, .3f, .3f, 1);
//        Color hightlightColor = new Color(0, .3f, .5f, .5f);

//        // Reverse the order of nodes because the last node to draw will be on top of other nodes and I want the first node to be on top
//        for (int i = graph.nodes.Count - 1; i >= 0; i--)
//        {
//            Rect box = new Rect(graph.nodes[i].box.position + zoomer.zoomOrigin, graph.nodes[i].box.size);
//            if (snapToGrid)
//            {
//                box.position = MathUtils.Round(box.position, gridSize);
//            }
//            EditorGUIHelper.DrawRect(box, boxColor, graph.nodes[i].state.Name);
//        }

//        if (currentSelectNode != null)
//        {
//            Rect box = new Rect(currentSelectNode.box.position + zoomer.zoomOrigin, currentSelectNode.box.size);
//            if (snapToGrid)
//            {
//                box.position = MathUtils.Round(box.position, gridSize);
//            }
//            EditorGUIHelper.DrawRect(box, boxColor, currentSelectNode.state.Name);
//            EditorGUIHelper.DrawOutline(box, 3, hightlightColor);
//        }
//    }

//    List<CustomLine> edges = new List<CustomLine>();
//    List<CustomLine> edgesForEachNode = new List<CustomLine>();
//    private void PaintEdges()
//    {
//        if (connectionStartNode != null)
//            Handles.DrawAAPolyLine(5, connectionStartNode.box.center - zoomer.zoomOrigin, Event.current.mousePosition);

//        foreach (var node in graph.nodes)
//        {
//            foreach (var transition in node.state.transitions)
//            {
//                if (transition.nextState)
//                {
//                    Vector2 startPos = zoomer.ConvertRealPosToScreenPos(node.box.center);
//                    Vector2 endPos = zoomer.ConvertRealPosToScreenPos(graph.GetNodeByState(transition.nextState).box.center);

//                    CustomLine edge = new CustomLine(startPos, endPos, lineSize);
//                    if (edgesForEachNode.Contains(edge)) // Check for duplicate transition to the same node
//                    {
//                        continue; // TODO: Draw multiple arrow like in the animator window when you have multiple transitions transit to the same state
//                    }
//                    edgesForEachNode.Add(edge);
//                    if (edges.Exists(e => e.Overlaps(edge)))
//                    {
//                        edge.start += Vector2.one * 10;
//                        edge.end += Vector2.one * 10;
//                    }
//                    edges.Add(edge);
//                }
//            }
//        }

//        foreach (var edge in edges)
//        {
//            Handles.DrawAAPolyLine(lineSize, edge.start, edge.end);
//            DrawArrow(edge.start, edge.end);
//        }
//        edges.Clear();
//        edgesForEachNode.Clear();
//    }

//    private void DrawArrow(Vector2 startPos, Vector2 endPos)
//    {
//        Vector2 center = MathUtils.Average(startPos, endPos);
//        Vector2 dir = (endPos - startPos).normalized;

//        // The 3 points of the equilateral triangle
//        Vector3 a = center + dir * 10;
//        Vector3 b = center + MathUtils.Rotate(dir, 120 * Mathf.Deg2Rad) * 10;
//        Vector3 c = center + MathUtils.Rotate(dir, -120 * Mathf.Deg2Rad) * 10;

//        // Change the point's coordinate from GUI's Coordinate ((0, 0) on the top left conner) to the normal coordinate ((0, 0) on the bottom left conner)
//        // Also, GL use the whole screen and clamp it to [0,1] but position doesn't include the window tab height so need to use Screen.height and Screen.safeArea
//        a.y = Screen.height - a.y;
//        b.y = Screen.height - b.y;
//        c.y = Screen.height - c.y;

//        // Remove the height of the window tab.
//        a.y -= 21;
//        b.y -= 21;
//        c.y -= 21;

//        // Return the value in [(0, 0), (1, 1)] because that what GL.Vertex use
//        a = MathUtils.InverseLerp(Vector2.zero, Screen.safeArea.size, a);
//        c = MathUtils.InverseLerp(Vector2.zero, Screen.safeArea.size, c);
//        b = MathUtils.InverseLerp(Vector2.zero, Screen.safeArea.size, b);

//        GameUtils.DrawGLTriangle(a, b, c);
//    }
//}