//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//[CreateAssetMenu(menuName = "Enemy/State/Normal")]
//public class EnemyState : ScriptableObject
//{
//    public string Name; // For displaying in the graph editor
//    public string enterAnimation;

//    public bool repeat;
//    public bool wait;
//    public bool random;

//    [ShowWhen("repeat")] public IntReference maxRepeatCount;
//    private int maxRepeatValue;
//    private int repeatCount;

//    [ShowWhen("wait")] public FloatReference waitTime;
//    private float waitTimeValue;

//    [ShowWhen("random")] public float[] probabilities;
//    private int transitionIndex;

//    [HideInInspector] public float elapsedTime;
//    public EnemyAction[] actions;
//    public List<EnemyTransition> transitions = new List<EnemyTransition>();

//    public void Enter(Enemy enemy)
//    {
//        elapsedTime = 0;

//        if (enterAnimation != "")
//            enemy.anim.Play(enterAnimation);
//        if (wait)
//            waitTimeValue = Time.time + waitTime;
//        if (repeat)
//        {
//            repeatCount = 0;
//            maxRepeatValue = maxRepeatCount;
//        }
//        if (random)
//            transitionIndex = MathUtils.Choose(probabilities);
//    }

//    public EnemyState UpdateState(Enemy enemy)
//    {
//        elapsedTime += Time.deltaTime;

//        if ((wait && Time.time >= waitTimeValue) || !wait)
//        {
//            DoActions(enemy);
//            waitTimeValue = Time.time + waitTime;
//        }
//        else
//            return null;

//        if (repeat)
//        {
//            repeatCount++;
//            if (repeatCount >= maxRepeatValue)
//                return CheckTransitions(enemy);
//        }
//        else
//            return CheckTransitions(enemy);

//        return null;
//    }

//    private void DoActions(Enemy enemy)
//    {
//        for (int i = 0; i < actions.Length; i++)
//        {
//            actions[i].Act(enemy);
//        }
//    }

//    protected EnemyState CheckTransitions(Enemy enemy)
//    {
//        if (random && IsTransitionValid(enemy, transitions[transitionIndex]))
//            return transitions[transitionIndex].nextState;

//        for (int i = 0; i < transitions.Count; i++) // Top transition get higher priority
//        {
//            if (IsTransitionValid(enemy, transitions[i]))
//                return transitions[i].nextState;
//        }
//        return null;
//    }

//    protected bool IsTransitionValid(Enemy enemy, EnemyTransition transition)
//    {
//        if (transition.Result(enemy) && transition.nextState != this && transition.nextState != null)
//            return true;
//        return false;
//    }
//}