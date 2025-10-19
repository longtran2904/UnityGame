using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MenuType
{
    None,
    Pause,
    Loading,
    Save,
    Option,
    Gameplay,
    Video,
    Audio,
    Control,
    // Credits,
    // Collectable
    // Equipment
    // Tutorial and tips
    // Character
    // You have unsaved changes
    
    Count
}

public class GameMenu : MonoBehaviour
{
    [System.Serializable]
    struct MenuInspector
    {
        public MenuType type;
        public GameObject menu;
    }
    
    
    [Header("Menu")]
    // @RANT: This is only for the inspector because Unity can't serialize a f****** dictionary
    [SerializeField] private MenuInspector[] gameMenus;
    private Dictionary<MenuType, GameObject> menus;
    private Stack<MenuType> openedMenus;
    
    [Header("Settings")]
    public GameSettings defaultSettings;
    public GameSettings currentSettings;
    public GameSettings tempSettings;
    public Toggle vsyncToggle;
    
    public bool loop;
    private Dictionary<MenuType, SwipeMenu[]> swipeMenus;
    private int currentSwipe;
    
    private void Start()
    {
        menus = new Dictionary<MenuType, GameObject>((int)MenuType.Count)
        {
            [MenuType.None] = null
        };
        foreach (MenuInspector menu in gameMenus)
            menus[menu.type] = menu.menu;
        openedMenus = new Stack<MenuType>((int)MenuType.Count);
        
        vsyncToggle.isOn = QualitySettings.vSyncCount > 0;
        vsyncToggle.onValueChanged.AddListener(enable =>
                                               {
                                                   tempSettings.vsync = enable;
                                                   QualitySettings.vSyncCount = enable ? 1 : 0;
                                               });
        
        swipeMenus = new Dictionary<MenuType, SwipeMenu[]>((int)MenuType.Count);
        foreach (MenuType type in menus.Keys)
        {
            if (type == MenuType.None)
            {
                swipeMenus[MenuType.None] = new SwipeMenu[0];
                continue;
            }
            
            swipeMenus[type] = menus[type].GetComponentsInChildren<SwipeMenu>();
            foreach (SwipeMenu swipe in swipeMenus[type])
                InitSwipeMenu(swipe);
            
            void InitSwipeMenu(SwipeMenu menu)
            {
                switch (menu.type)
                {
                    case SwipeSetting.Resolution:
                    {
                        menu.InitSwipeMenu(Screen.resolutions, r => r.CamelCase(),
                                           i => tempSettings.resolution = Screen.resolutions[i], r => GameSettings.CompareResolution(r, Screen.currentResolution));
                    }
                    break;
                    case SwipeSetting.ScreenMode:
                    {
                        menu.InitSwipeMenu<FullScreenMode>(GameSettings.fullScreenModeCount, mode => mode.CamelCase(),
                                                           i => tempSettings.mode = (FullScreenMode)i, mode => mode == Screen.fullScreenMode);
                    }
                    break;
                }
            }
        }
        
        // TODO: Have a way to save the currentSettings and defaultSettings when the game restarts or gets played later
        currentSettings.Copy(tempSettings);
        defaultSettings.Copy(tempSettings);
    }
    
    private void Update()
    {
        if (openedMenus.Count > 0)
        {
            int input = (int)GameInput.GetAxis(AxisType.Vertical, true);
            if (input != 0)
            {
                MenuType current = openedMenus.Peek();
                int nextIndex = MathUtils.LoopIndex(currentSwipe + input, swipeMenus[current].Length, loop);
                if (nextIndex != currentSwipe)
                {
                    swipeMenus[current][currentSwipe].Highlight(false);
                    swipeMenus[current][nextIndex].Highlight(true);
                    currentSwipe = nextIndex;
                }
            }
            
            if (GameInput.GetRawInput(InputType.Menu))
                CloseCurrentMenu();
        }
        else if (GameInput.GetRawInput(InputType.Menu))
            OpenMenu(MenuType.Pause);
    }
    
    public void SetCurrentSwipe(SwipeMenu swipe)
    {
        int i = 0;
        foreach (SwipeMenu menu in swipeMenus[openedMenus.Peek()])
        {
            if (swipe == menu)
            {
                currentSwipe = i;
                return;
            }
            ++i;
        }
        Debug.LogError($"Can't set {swipe.name} to the current swipe!");
    }
    
    public void OpenMenu(MenuType type)
    {
        if (openedMenus.Contains(type))
        {
            Debug.LogError($"The {type} menu is already opened!");
            return;
        }
        
        switch (type)
        {
            case MenuType.None:
            return;
            case MenuType.Pause:
            {
                Time.timeScale = 0f;
                GameInput.EnableAllInputs(false);
                // TODO: Blur the background
            } break;
        }
        
        MenuType currentMenu = openedMenus.Count > 0 ? openedMenus.Peek() : MenuType.None;
        menus[currentMenu]?.SetActive(false);
        openedMenus.Push(type);
        menus[type].SetActive(true);
        
        // TODO: Highlight(false) all the objects in the current menu and the new menu
    }
    
    void ApplySettings(GameSettings oldSettings, GameSettings newSettings, bool closeCurrentMenu)
    {
        if (GameSettings.CompareSettings(oldSettings, newSettings))
            goto END;
        
        oldSettings.Copy(newSettings);
        
        // NOTE: Applying Settings
        {
            foreach (SwipeMenu[] swipes in swipeMenus.Values)
            {
                foreach (SwipeMenu swipe in swipes)
                {
                    switch (swipe.type)
                    {
                        case SwipeSetting.Resolution:
                        {
                            swipe.LoopAndSetCurrent(i => GameSettings.CompareResolution(Screen.resolutions[i], newSettings.resolution), true);
                        }
                        break;
                        case SwipeSetting.ScreenMode:
                        {
                            swipe.LoopAndSetCurrent(i => newSettings.mode == (FullScreenMode)i, true);
                        }
                        break;
                    }
                }
            }
            
            if (vsyncToggle.isOn != newSettings.vsync)
                vsyncToggle.isOn = newSettings.vsync;
        }
        
        END:
        if (closeCurrentMenu)
            CloseCurrentMenu();
        
        // TODO: Open a "Do you want to discard unsaved changes?" pop-up
        // OpenMenu(MenuType.UnsavedChanges, bool closePrevMenu);
        // if (ClickDiscarded())
        // {
        //     tempSettings.Copy(currentSettings);
        //     ApplySettings(tempSettings);
        //     CloseCurrentMenu();
        //     CloseCurrentMenu();
        // }
        // else if (ClickNo())
        // {
        //     CloseCurrentMenu();
        // }
    }
    
#region Call by UnityEvent
    public void CloseCurrentMenu()
    {
        MenuType currentMenu = openedMenus.Count > 0 ? openedMenus.Pop() : MenuType.None;
        if (currentMenu == MenuType.Pause)
        {
            GameInput.EnableAllInputs(true);
            Time.timeScale = 1f;
        }
        menus[currentMenu]?.SetActive(false);
        
        MenuType prevMenu = openedMenus.Count > 0 ? openedMenus.Peek() : MenuType.None;
        menus[prevMenu]?.SetActive(true);
    }
    
    public void Confirm()
    {
        ApplySettings(currentSettings, tempSettings, false);
    }
    
    public void ResetToDefault()
    {
        ApplySettings(tempSettings, defaultSettings, false);
    }
    
    public void Cancel()
    {
        ApplySettings(tempSettings, currentSettings, true);
    }
    
    public void Quit()
    {
        Application.Quit();
    }
#endregion
}