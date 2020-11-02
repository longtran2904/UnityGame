using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalItem : Item
{
    public State state;

    protected override void Use() { }

    protected void AddStateToEnemies(Enemies[] enemies, State state)
    {
        foreach (Enemies enemy in enemies)
        {
            AddStateToEnemy(enemy, state);
        }
    }

    protected void AddStateToEnemy(Enemies enemy, State state)
    {
        StateManager.AddStateToEnemy(enemy, state);
    }
}
