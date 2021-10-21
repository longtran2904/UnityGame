#if UNITY_EDITOR
//#define DEBUG_SEARCH
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawner : MonoBehaviour
{
    public SpawnerInfo spawner;
    public GameEvent endWaves;

#if UNITY_EDITOR
    [Header("DEBUG_SEARCH")]
    public TileBase debugGroundTile;
    public TileBase debugEmptyTile;
    public TileBase debugFailTile;
    private Tilemap debugTilemap;
#endif

    [HideInInspector] public int waveCount;
    private List<Vector3Int>[] spawnPos = new List<Vector3Int>[(int)SpawnLocation.LocationCount];
    private Tilemap tilemap;

    private void Start()
    {
        waveCount = spawner.numberOfWaves.randomValue;

        tilemap = transform.parent.GetChild(0).GetChild(2).GetComponent<Tilemap>();
#if DEBUG_SEARCH
        GameObject debugTilemapObj = new GameObject("Debug", typeof(Tilemap), typeof(TilemapRenderer));
        debugTilemapObj.transform.parent = tilemap.transform.parent;
        debugTilemapObj.transform.localPosition = Vector3.zero;
        debugTilemap = debugTilemapObj.GetComponent<Tilemap>();
        debugTilemapObj.GetComponent<TilemapRenderer>().sortingOrder = 10;
#endif

        for (int i = 0; i < spawnPos.Length; i++)
        {
            spawnPos[i] = new List<Vector3Int>();
        }

        for (int y = 0; y <= tilemap.cellBounds.size.y - 3; y++)
        {
            for (int x = 0; x <= tilemap.cellBounds.size.x - 3; x++)
            {
                Vector3Int pos = tilemap.cellBounds.min + new Vector3Int(x, y, 0);
                int pattern = 0;
                for (int j = 0; j < 3; j++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (tilemap.HasTile(pos + new Vector3Int(i, j, 0)))
                        {
                            pattern |= 1 << (i + j * 3);
                        }
                    }
                }

                Vector3Int offset = new Vector3Int(1, 1, 0);
                TileBase debugTile = debugFailTile;
                if (pattern == 0b000000111)
                {
                    spawnPos[(int)SpawnLocation.Ground].Add(pos + offset);
                    debugTile = debugGroundTile;
                    
                }
                else if (pattern == 0)
                {
                    spawnPos[(int)SpawnLocation.Empty].Add(pos + offset);
                    debugTile =  debugEmptyTile;
                }
#if DEBUG_SEARCH
                debugTilemap.SetTile(pos + offset, debugTile);
#endif
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Enemy.numberOfEnemiesAlive <= 0 && waveCount > 0)
        {
            waveCount--;
            SpawnEnemy(spawner.numberOfEnemies.randomValue);
        }
        else if (Enemy.numberOfEnemiesAlive == 0)
        {
            endWaves.Raise();
            Destroy(gameObject);
        }
    }

    // TODO: Enemies don't spawn close to doors
    private void SpawnEnemy(int enemyCount)
    {
        List<Vector3> usedPos = new List<Vector3>(enemyCount - 1);
        Vector3 pos;
        for (int i = 0; i < enemyCount; i++)
        {
            EnemyInfo enemy = spawner.info.RandomElement();
            do
            {
                pos = spawnPos[(int)enemy.spawnLocation].RandomElement() + tilemap.transform.position + new Vector3(.5f, .5f, 0);
            } while (Physics2D.BoxCast(pos, Vector2.one * 3, 0, Vector2.zero, 0) && usedPos.Contains(pos));
            usedPos.Add(pos);
            Instantiate(enemy.enemy, pos, Quaternion.identity);
        }
    }
}
