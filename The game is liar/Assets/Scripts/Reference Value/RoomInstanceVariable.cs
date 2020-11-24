using UnityEngine;
using ProceduralLevelGenerator.Unity.Generators.Common.Rooms;

[CreateAssetMenu(menuName = "Variable/RoomInstance")]
public class RoomInstanceVariable : ScriptableObject
{
    public RoomInstance value;
}
