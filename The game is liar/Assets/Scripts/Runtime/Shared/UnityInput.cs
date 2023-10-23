using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeyInput // RANT: This is a class because I hate C# (Dictionary/List can't modify a struct)
{
    public KeyCode code;
    public KeyTriggerType trigger;
    public bool enable;
    
    public KeyInput(KeyCode code, KeyTriggerType trigger)
    {
        this.code = code;
        enable = true;
        this.trigger = trigger;
    }
}

public enum KeyTriggerType
{
    Down,
    Hold,
    Up
}

[System.Serializable]
public struct InputKey // RANT: Because Unity is stupid and can't serialize a fucking dictionary
{
    public InputType type;
    public KeyCode code;
    public KeyTriggerType trigger;
}

public enum AxisType
{
    Horizontal,
    Vertical,
}

public class UnityInput : MonoBehaviour
{
    public InputKey[] keys;
    private Dictionary<InputType, KeyInput> inputs;
    const int maxLevelInput = 3;
    private bool disableAllInputs;
    private bool[] disableMouseInputs;
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        inputs = new Dictionary<InputType, KeyInput>(keys.Length + 1);
        disableMouseInputs = new bool[maxLevelInput];
        inputs[InputType.None] = new KeyInput(KeyCode.None, KeyTriggerType.Down);
        foreach (InputKey key in keys)
            inputs[key.type] = new KeyInput(key.code, key.trigger);
        GameInput.Init(this);
    }
    
    public void EnableMouseInput(bool enable, int level)
    {
        Debug.Assert(MathUtils.InRange(level, 0, maxLevelInput - 1));
        disableMouseInputs[level] = !enable;
    }
    
    public void EnableAllInputs(bool enable)
    {
        disableAllInputs = !enable;
    }
    
    public void EnableInput(InputType type, bool enable)
    {
        inputs[type].enable = enable;
    }
    
    public bool GetInput(InputType type)
    {
        GameDebug.Assert(inputs.ContainsKey(type), type);
        if (disableAllInputs || !inputs[type].enable)
            return false;
        return GetRawInput(type);
    }
    
    public bool GetRawInput(InputType type)
    {
        switch (inputs[type].trigger)
        {
            case KeyTriggerType.Down: return Input.GetKeyDown(inputs[type].code);
            case KeyTriggerType.Hold: return Input.GetKey(inputs[type].code);
            case KeyTriggerType.Up:   return Input.GetKeyUp(inputs[type].code);
            default: Debug.LogError($"Input type: {type} is invalid!");
            return false;
        }
    }
    
    public Vector2 GetMousePos()
    {
        return disableAllInputs || disableMouseInputs[0] ? Vector2.zero : (Vector2)Input.mousePosition;
    }
    
    public Vector2 GetMouseWorldPos()
    {
        return mainCamera.ScreenToWorldPoint(GetMousePos());
    }
    
    public float GetMouseWheel()
    {
        return disableAllInputs || disableMouseInputs[0] ? 0 : Input.mouseScrollDelta.y;
    }
    
    public Vector2 GetDirToMouse(Vector2 pos, int level)
    {
        return disableAllInputs || disableMouseInputs[level] ? Vector2.zero : ((Vector2)mainCamera.ScreenToWorldPoint(Input.mousePosition) - pos);
    }
    
    public Vector2 GetMouseDir(int level)
    {
        if (disableAllInputs || disableMouseInputs[level])
            return Vector2.zero;
        
        Vector2 mousePos = Input.mousePosition;
        Vector2 dir = MathUtils.Clamp01(new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height)) * 2f - Vector2.one;
        return dir;
    }
    
    public float GetAxis(AxisType type, bool raw)
    {
        return (disableAllInputs && !raw) ? 0 : Input.GetAxisRaw(type.ToString());
    }
    
    public bool IsMouseOnScreen()
    {
        // NOTE: Currently, this check for disableAllInputs is useless.
        return !disableAllInputs && Screen.safeArea.Contains(Input.mousePosition);
    }
}