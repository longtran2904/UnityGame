using UnityEngine;

[System.Serializable]
public class StateNode
{
    public Rect box;
    public EnemyState state;

    public StateNode(Vector2 pos, Vector2 size, EnemyState state, string name) : this(pos, size, state)
    {
        this.state.name = name;
    }

    public StateNode(Vector2 pos, Vector2 size, EnemyState state)
    {
        box = new Rect(pos, size);
        this.state = state;
    }

    public CustomEvent HandleEvent(Event e)
    {
        if (true)
        {
            switch (e.type)
            {
                case EventType.MouseDrag:
                    if (box.Contains(e.mousePosition) && e.button == 0)
                    {
                        box.position += e.delta;
                        return CustomEvent.Select;
                    }
                    break;
                case EventType.MouseDown:
                    if (box.Contains(e.mousePosition))
                        return CustomEvent.Select;
                    break;
                case EventType.ContextClick:
                    if (box.Contains(e.mousePosition))
                        return CustomEvent.ContextMenu;
                    break;
            } 
        }
        return CustomEvent.None;
    }

    public void Paint()
    {
        GUI.Box(box, state?.Name);
    }
}

public enum CustomEvent
{
    None,
    ContextMenu,
    Select
}
