using UnityEngine;

public enum SpawnLocation
{
    Ground,
    Empty,

    LocationCount
}

[System.Serializable]
public struct EnemyInfo
{
    public Enemy enemy;
    public SpawnLocation spawnLocation;
}

[CreateAssetMenu(menuName = "Data/Spawner Info")]
public class SpawnerInfo : ScriptableObject
{
    [MinMax(1, 5)] public RangedInt numberOfWaves;
    [MinMax(1, 20)] public RangedInt numberOfEnemies;
    public Optional<EnemyInfo>[] info;
}
