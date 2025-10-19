using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public enum TransitionType
{
    None,
    Move,
    Scale,
    Color,
}

public class GameButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TransitionType normal;
    [ShowWhen("normal", new object[] { TransitionType.Move, TransitionType.Scale })] public Vector2 normalValue;
    [ShowWhen("normal", TransitionType.Color)] public Color normalColor;
    public TransitionType hover;
    [ShowWhen("hover", new object[] { TransitionType.Move, TransitionType.Scale })] public Vector2 hoverValue;
    [ShowWhen("hover", TransitionType.Color)] public Color hoverColor;
    public TransitionType click;
    [ShowWhen("click", new object[] { TransitionType.Move, TransitionType.Scale })] public Vector2 clickValue;
    [ShowWhen("click", TransitionType.Color)] public Color clickColor;

    [Header("On Click")]
    public MenuType openMenuOnClick;
    private GameMenu menu;

    public UnityEvent onClick;
    private MaskableGraphic[] UIObjects;

    void Start()
    {
        menu = GetComponentInParent<GameMenu>();
        UIObjects = GetComponents<MaskableGraphic>();
        ChangeButton(normal, normalValue, normalColor);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ChangeButton(click, clickValue, clickColor);
        menu?.OpenMenu(openMenuOnClick);
        onClick?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ChangeButton(hover, hoverValue, hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ChangeButton(normal, normalValue, normalColor);
    }

    private void ChangeButton(TransitionType type, Vector2 value, Color color)
    {
        switch (type)
        {
            case TransitionType.Move:
                transform.position = value;
                break;
            case TransitionType.Scale:
                transform.localScale = (Vector3)value + Vector3.forward;
                break;
            case TransitionType.Color:
                foreach (MaskableGraphic ui in UIObjects)
                    ui.color = color;
                break;
        }
    }
}
