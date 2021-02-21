using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/State Graph")]
public class StateGraph : ScriptableObject
{
    public List<EnemyState> states = new List<EnemyState>();
    public List<StateNode> nodes = new List<StateNode>();
    public List<StateEdge> edges = new List<StateEdge>();
}
