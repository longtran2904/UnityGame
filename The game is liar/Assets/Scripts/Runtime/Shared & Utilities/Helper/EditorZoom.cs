using UnityEngine;

/*
 * NOTE: mousePosition won't change when zoom in or out so need to use ConvertScreenCoordsToZoomCoords
 * 
 * How to use:
 * Create an instacne of editor zoom
 * Call EditorZoom.Begin()
 * Draw zoom area here. Offset with zoomOrigin.
 * Call EditorZoom.End()
 * Draw non zoom area here.
 * Ex:
 * Zoom zoomer = new Zoom();
 * void OnGui()
 * {
 *      zoomer.Begin();
 *      Rect rect = new Rect(node.position + zoomer.zoomOrigin, node.size);
 *      Gui.Box(rect, "");
 *      zoomer.End();
 *      // Draw non zoom
 * }
 */
public class EditorZoom
{
    private const float editorWindowTabHeight = 21.0f;

    public bool shouldZoomTowardsMouse = true; //if this is false, it will always zoom towards the center of the content (0,0)
    public float zoom = 1f;

    public Rect zoomArea;
    public Vector2 zoomOrigin;
    private Matrix4x4 prevMatrix;

    /// <summary>
    /// Begin a zoom area with a predefine size.
    /// </summary>
    public Rect Begin(Rect screenArea)
    {
        zoomArea = screenArea;
        return Begin();
    }

    /// <summary>
    /// Begin a zoom area that cover all the screen.
    /// </summary>
    public Rect Begin(params GUILayoutOption[] options)
    {
        // fill the available area
        // NOTE: GUILayoutUtility.GetRect(minWidth, maxWidth, minHeight, maxHeight, options)) will reserve layout space for a flexible rect.
        // If call this multiple times a frame then all rects will split even to fit the screen.
        // Change this to the fixed size version (i.e GetRect(width, height, options)) to prevent this.
        var possibleZoomArea = GUILayoutUtility.GetRect(0, 10000, 0, 10000, options);
        if (Event.current.type == EventType.Repaint) //the size is correct during repaint, during layout it's (0, 0, 1, 1)
            zoomArea = possibleZoomArea;
        return Begin();
    }

    private Rect Begin()
    {
        HandleEvents();

        GUI.EndGroup(); // End the group Unity begins automatically for an EditorWindow to clip out the window tab. This allows us to draw outside of the size of the EditorWindow.

        Rect clippedArea = zoomArea.ScaleSizeBy(1f / zoom, zoomArea.TopLeft());
        clippedArea.y += editorWindowTabHeight;
        GUI.BeginGroup(clippedArea);

        prevMatrix = GUI.matrix;
        Matrix4x4 translation = Matrix4x4.TRS(clippedArea.TopLeft(), Quaternion.identity, Vector3.one);
        Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoom, zoom, 1.0f));
        GUI.matrix = translation * scale * translation.inverse * GUI.matrix;

        return clippedArea;
    }

    public void End()
    {
        GUI.matrix = prevMatrix; //restore the original matrix
        GUI.EndGroup();
        GUI.BeginGroup(new Rect(0.0f, editorWindowTabHeight, Screen.width, Screen.height));
    }

    /// <summary>
    /// Convert a point on the screen to the real point.
    /// </summary>
    /// <param name="screenPos">screenPos is the position relative to the screen, e.g Event.current.mousePosition</param>
    /// <returns>Return the position of the point when zoom is 1 and zoomOrigin is (0; 0).</returns>
    public Vector2 ConvertScreenPosToRealPos(Vector2 screenPos)
    {
        return screenPos / zoom - zoomOrigin;
    }

    /// <summary>
    /// The opposite of ConvertScreenPosToRealPos().
    /// </summary>
    /// <param name="realPos">The actual position when zoom is 1 and zoomOrigin is (0; 0).</param>
    /// <returns>Return the position relative to the current zoom and zoomOrigin.</returns>
    public Vector2 ConvertRealPosToScreenPos(Vector2 realPos)
    {
        return (realPos + zoomOrigin) * zoom;
    }

    public void HandleEvents()
    {
        if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
        {
            zoomOrigin += Event.current.delta / zoom;
            Event.current.Use();
        }

        if (Event.current.type == EventType.ScrollWheel)
        {
            float oldZoom = zoom;
            float zoomDelta = -Event.current.delta.y / 6; // Event.current.delta will be positive when scroll down and negative when scroll up
            zoom += zoomDelta;
            zoom = Mathf.Clamp(zoom, 0.1f, 10f);

            Vector2 zoomToPos = shouldZoomTowardsMouse ? Event.current.mousePosition : zoomArea.size / 2;
            zoomOrigin -= (zoomToPos - zoomArea.TopLeft()) / oldZoom - (oldZoom / zoom) * ((zoomToPos - zoomArea.TopLeft()) / oldZoom);

            Event.current.Use();
        }
    }
}
