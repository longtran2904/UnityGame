using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using TMPro;

public enum SwipeSetting
{
    None,
    Resolution,
    ScreenMode,
    // Scailing mode
    // Quality mode
    // Post-processing mode
    // Lighting and Shadow mode

    Count
}

public class SwipeMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SwipeSetting type;
    public Color normalColor;
    public Color highlightColor;
    public bool loop;

    [HideInInspector] public bool highlight;
    [HideInInspector] public TextMeshProUGUI title;

    private System.Collections.Generic.List<string> optionNames;
    private int current;
    private TextMeshProUGUI content;
    private Image leftArrow;
    private Image rightArrow;
    private Action<int> onSwipe;
    private GameMenu gameMenu;

    void Update()
    {
        if (highlight)
            Swipe((int)GameInput.GetAxis(AxisType.Horizontal, true));
    }

    void Swipe(int swipe)
    {
        if (swipe == 0)
            return;

        int i = MathUtils.LoopIndex(current + swipe, optionNames.Count, loop);
        if (current == i)
            return;

        SetCurrent(i);
    }

    public void LoopAndSetCurrent(Func<int, bool> isCurrent, bool different)
    {
        for (int i = 0; i < optionNames.Count; i++)
            if (isCurrent(i))
            {
                if (!(different && i == current))
                    SetCurrent(i);
                return;
            }
    }

    void SetCurrent(int index)
    {
        current = index;
        content.text = optionNames[current];
        onSwipe?.Invoke(current);
    }

    public void InitSwipeMenu<T>(T[] options, Func<T, string> format, Action<int> onSwipe, Func<T, bool> isCurrent = null)
    {
        Init(options.Length, i => format(options[i]), onSwipe, i => isCurrent(options[i]));
    }

    public void InitSwipeMenu<T>(int count, Func<T, string> format, Action<int> onSwipe, Func<T, bool> isCurrent = null) where T : Enum
    {
        Init(count, i => format((T)(object)i), onSwipe, i => isCurrent((T)(object)i));
    }

    void Init(int count, Func<int, string> format, Action<int> onSwipe, Func<int, bool> isCurrent)
    {
        title = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        title.text = type.ToString();

        content = transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();
        leftArrow = transform.GetChild(1).GetChild(0).GetComponent<Image>();
        rightArrow = transform.GetChild(1).GetChild(2).GetComponent<Image>();
        leftArrow.GetComponent<GameButton>().onClick.AddListener(() => Swipe(-1));
        rightArrow.GetComponent<GameButton>().onClick.AddListener(() => Swipe(1));

        gameMenu = GetComponentInParent<GameMenu>();
        optionNames = new System.Collections.Generic.List<string>(count);
        Highlight(false);

        for (int i = 0; i < count; ++i)
        {
            string optionName = format(i);
            optionNames.Add(optionName);
            if (isCurrent != null && isCurrent(i))
                current = i;
        }

        content.text = optionNames[current];
        this.onSwipe = onSwipe;
    }

    public void Highlight(bool highlight)
    {
        this.highlight = highlight;
        leftArrow.color = rightArrow.color = content.color = title.color = (highlight ? highlightColor : normalColor);
        if (highlight)
            gameMenu.SetCurrentSwipe(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Highlight(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Highlight(false);
    }
}
