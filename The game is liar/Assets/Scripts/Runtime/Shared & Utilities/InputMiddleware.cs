using UnityEngine;

public abstract class InputMiddleware : ScriptableObject
{
    // Return true when the middleware want to invoke jump
    public abstract bool Process(InputState input);
}
