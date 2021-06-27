//using System;

//public enum RuleType
//{
//    Alien,
//    Jellyfish,
//    Maggot
//}

//public enum ChooseActionType { Repeat, Shuffle }

//class EnemyRule
//{
//    public int priority;

//    public int numberOfTags
//    {
//        get => All.Length + Any.Length + None.Length;
//    }
//    private EnemyTag[] All;
//    private EnemyTag[] Any;
//    private EnemyTag[] None;

//    private ChooseActionType actionType;
//    private int currentAction;
//    //private IEnemyAction[] actions;
//    private EnemyState[] states;

//    public EnemyRule(int priority, ChooseActionType actionType = ChooseActionType.Repeat, EnemyTag[] All = null, EnemyTag[] Any = null, EnemyTag[] None = null, EnemyState[] states = null)
//    {
//        this.priority   = priority;
//        this.All        = All     ?? Array.Empty<EnemyTag>();
//        this.Any        = Any     ?? Array.Empty<EnemyTag>();
//        this.None       = None    ?? Array.Empty<EnemyTag>();
//        this.states     = states ?? Array.Empty<EnemyState>();
//        this.actionType = actionType;
//    }

//    public bool CheckTags(Enemy enemy)
//    {
//        /*bool all = true, any = true, none = true;

//        // 'All' tags check
//        foreach (var tag in All)
//        {
//            if (!enemy.CheckTag(tag))
//            {
//                all = false;
//                break;
//            }
//        }

//        // 'Any' tags check
//        if (Any.Length > 0) any = false;
//        foreach (var tag in Any)
//        {
//            if (enemy.CheckTag(tag))
//            {
//                any = true;
//                break;
//            }
//        }

//        // 'None' tags check
//        foreach (var tag in None)
//        {
//            if (enemy.CheckTag(tag))
//                none = false;
//                break;
//        }

//        return all && any && none;*/
//        return false;
//    }

//    public EnemyState GetState()
//    {
//        int prevState = currentAction;
//        int nextState = currentAction + 1;
//        currentAction = nextState < states.Length - 1 ? nextState : 0;
//        if (actionType == ChooseActionType.Shuffle && nextState > states.Length - 1)
//            MathUtils.Shuffle(states);
//        return states[prevState];
//    }
//}
