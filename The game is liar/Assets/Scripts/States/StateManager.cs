using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StateManager : MonoBehaviour
{
    public static void AddStateToEnemy(Enemies enemy, State state)
    {
        //Destroy(Instantiate(GameAssets.instance.stateParticles[(int)state.type], enemy.transform.position, Quaternion.identity, enemy.transform));
        if (state.type == StatusType.Freeze || state.type == StatusType.Slow)
        {
            enemy.StartCoroutine(EnemyStatModify(state.duration, enemy.GetComponent<EnemiesMovement>().speed, state.percent, x => { enemy.GetComponent<EnemiesMovement>().speed = x; }));
        }
        else if (state.type == StatusType.Injured)
        {
            enemy.StartCoroutine(EnemyStatModify(state.duration, enemy.damage, state.percent, x => { enemy.damage = (int)x; }));
        }
        else if (state.type == StatusType.Blind)
        {
            enemy.StartCoroutine(EnemyStatModify(state.duration, enemy.GetComponent<EnemiesMovement>().attackRange, state.percent, x => { enemy.GetComponent<EnemiesMovement>().attackRange = x; }));
        }
        else
        {
            enemy.StartCoroutine(DamageOverTime(enemy, state.damage, state.duration, state.timeBtwHits));
        }
    }

    private static IEnumerator DamageOverTime(Enemies enemy, int damage, float duration, float timeBtwHits)
    {
        while (duration > 0)
        {
            enemy.Hurt(damage);
            duration -= timeBtwHits;
            yield return new WaitForSeconds(timeBtwHits);
        }
    }

    private static IEnumerator EnemyStatModify(float duration, float normalValue, float percent, Action<float> modifyStat)
    {
        modifyStat(normalValue * (1 - percent));
        yield return new WaitForSeconds(duration);
        modifyStat(normalValue);
    }
}
