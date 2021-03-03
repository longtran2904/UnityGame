using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/State Graph")]
public class StateGraph : ScriptableObject
{
    public string pathToSaveNewState = "Assets/Files/Enemy/Enemy States";
    public List<StateNode> nodes = new List<StateNode>();
    public List<EnemyState> temporaryState = new List<EnemyState>(); // List of custom new states that create through context menu in the window
    public AnyState anyState;
    private Dictionary<EnemyState, StateNode> stateCache = new Dictionary<EnemyState, StateNode>();

    public void ClearCache()
    {
        stateCache.Clear();
    }

    public StateNode GetNodeByState(EnemyState state)
    {
        if (stateCache.ContainsKey(state))
            return stateCache[state];

        foreach (var node in nodes)
        {
            if (node.state == state)
            {
                stateCache.Add(node.state, node);
                return node;
            }
        }
        return null;
    }
}
