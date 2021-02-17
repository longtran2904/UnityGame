using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalItem : Item
{
    public State state;

    protected override void Use() { }

    protected void AddStateToEnemies(Enemy[] enemies, State state)
    {
        foreach (Enemy enemy in enemies)
        {
            AddStateToEnemy(enemy, state);
        }
    }

    protected void AddStateToEnemy(Enemy enemy, State state)
    {
        StateManager.AddStateToEnemy(enemy, state);
    }
}
