using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GrenadeEffect : ScriptableObject
{
    public abstract void Explode(Grenade grenade, Vector2 pos);
}
