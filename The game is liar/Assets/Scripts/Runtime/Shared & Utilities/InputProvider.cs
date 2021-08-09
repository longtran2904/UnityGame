using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Input Provider", menuName = "Input/Provider")]
public class InputProvider : ScriptableObject
{
    public InputMiddleware[] middlewares;
    public event Action jumpAction;

    public InputState GetState()
    {
        InputState input = new InputState();
        foreach (var middleware in middlewares)
        {
            bool requestJump = middleware.Process(input);
            if (requestJump && input.canJump)
            {
                jumpAction?.Invoke();
            }
        }
        return input;
    }
}