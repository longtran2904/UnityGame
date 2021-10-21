using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorGUIHelper
{
    public static GUIStyle boxStyle;

    public static void DrawRect(float x, float y, float width, float height, Color color, string text = null)
    {
        DrawRect(new Rect(x, y, width, height), color, text);
    }

    public static void DrawRect(Rect rect, Color color, string text = null)
    {
        DrawRect(rect, color, new GUIContent(text));
    }

    public static void DrawRect(Rect rect, Color color, GUIContent content = null)
    {
        if (boxStyle == null)
        {
            boxStyle = GUI.skin.box;
            boxStyle.normal.background = Texture2D.whiteTexture;
        }
        Color backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = color;
        GUI.Box(rect, content ?? GUIContent.none, boxStyle);
        GUI.backgroundColor = backgroundColor;
    }

    public static void DrawOutline(Rect rect, float outlineSize, Color color)
    {
        DrawRect(rect.x, rect.y, rect.width, outlineSize, color);
        DrawRect(rect.x, rect.y, outlineSize, rect.height, color);
        DrawRect(rect.x, rect.y + rect.height, rect.width, outlineSize, color);
        DrawRect(rect.x + rect.width, rect.y, outlineSize, rect.height + outlineSize, color);
    }
}
