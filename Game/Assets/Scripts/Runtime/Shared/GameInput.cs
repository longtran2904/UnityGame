using System.Collections.Generic;
using UnityEngine;

public enum InputType
{
    None,
    Jump,
    Shoot,
    Melee,
    LeftItem,
    RightItem,
    Reload,
    Menu,
    Map,
    Interact,
    NextDialogue,
    
    Debug_CameraShake,
    Debug_ShakeIncrease,
    Debug_ShakeDecrease,
    Debug_CameraShock,
    
    Debug_ToggleLog,
    Debug_ClearLog,
    
    Debug_ResetTime,
    Debug_SlowTime,
    Debug_FastTime,
    
    Debug_SpawnParticle,
    Debug_TestPlayerVFX,
    
    Count
}

public enum GameEventType
{
    None,
    GameOver,
    EndRoom,
    NextRoom,
    
    //Reload,
    //Dialogue,
    
    Count
}

public static class GameInput
{
    private static UnityInput platformInput;
    private static Dictionary<GameEventType, System.Action<Transform>> events;
    private static Property<GameEventType> flags;
    
    public static void Init(UnityInput input)
    {
        events = new Dictionary<GameEventType, System.Action<Transform>>((int)GameEventType.Count);
        if (platformInput == null)
            platformInput = input;
        else
            Debug.LogError("Platform input is already initialized!");
    }
    
    public static bool GetInput(InputType type)
    {
        return platformInput.GetInput(type);
    }
    
    public static bool GetRawInput(InputType type)
    {
        return platformInput.GetRawInput(type);
    }
    
    public static void EnableInput(InputType type, bool enable)
    {
        platformInput.EnableInput(type, enable);
    }
    
    public static void EnableMouseInput(bool enable, int level)
    {
        platformInput.EnableMouseInput(enable, level);
    }
    
    public static void EnableAllInputs(bool enable)
    {
        platformInput.EnableAllInputs(enable);
    }
    
    public static float GetAxis(AxisType type, bool raw = false)
    {
        return platformInput.GetAxis(type, raw);
    }
    
    public static Vector2 GetAxis(bool raw = false)
    {
        return new Vector2(platformInput.GetAxis(AxisType.Horizontal, raw), platformInput.GetAxis(AxisType.Vertical, raw));
    }
    
    public static Vector2 GetMousePos()
    {
        return platformInput.GetMousePos();
    }
    
    public static Vector2 GetMouseWorldPos()
    {
        return platformInput.GetMouseWorldPos();
    }
    
    public static Vector2 GetDirToMouse(Vector2 pos, int level = 0)
    {
        return platformInput.GetDirToMouse(pos, level);
    }
    
    public static Vector2 GetMouseDir(int level = 0)
    {
        return platformInput.GetMouseDir(level);
    }
    
    public static float GetMouseWheel()
    {
        return platformInput.GetMouseWheel();
    }
    
    public static bool IsMouseOnScreen()
    {
        return platformInput.IsMouseOnScreen();
    }
    
    public static void BindEvent(GameEventType type, System.Action<Transform> func)
    {
        if (events.ContainsKey(type))
            events[type] += func;
        else
            events[type] = func;
    }
    
    public static void TriggerEvent(GameEventType type, Transform room)
    {
        Debug.Assert(!flags.HasProperty(type));
        events[type]?.Invoke(room);
        flags.SetProperty(type, true);
        // NOTE: This only works if TriggerEvent is called at the beginning of the frame. Currently, most of the calls are from GameManager which run first.
        platformInput.InvokeAfterFrames(-1, () => flags.SetProperty(type, false));
    }
    
    public static bool GetEvent(GameEventType type)
    {
        return flags.HasProperty(type);
    }
}