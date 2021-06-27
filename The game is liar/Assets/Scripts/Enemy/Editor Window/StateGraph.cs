//using System.Collections.Generic;
//using UnityEngine;

//[CreateAssetMenu(menuName = "Enemy/State Graph")]
//public class StateGraph : ScriptableObject
//{
//    public StateNode startNode;
//    public StateNode anyNode;
//    public List<StateNode> nodes = new List<StateNode>();
//    private Dictionary<EnemyState, StateNode> stateCache = new Dictionary<EnemyState, StateNode>();

//    public void ClearCache()
//    {
//        stateCache.Clear();
//    }

//    public StateNode GetNodeByState(EnemyState state)
//    {
//        if (stateCache.ContainsKey(state))
//            return stateCache[state];

//        foreach (var node in nodes)
//        {
//            if (node.state == state)
//            {
//                stateCache.Add(node.state, node);
//                return node;
//            }
//        }
//        return null;
//    }
//}
