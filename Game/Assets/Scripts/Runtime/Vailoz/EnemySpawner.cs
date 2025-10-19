#if UNITY_EDITOR
#define DEBUG_SEARCH
#endif

using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawner : MonoBehaviour
{
    // TODO: Collases all enemy spawner to a single instance (maybe a scriptable object?) and maybe replace waveCount and totalWaves with a IntVariable?
    public static int waveCount;
    public static int totalWaves;
    public SpawnerInfo spawner;

#if UNITY_EDITOR
    [Header("DEBUG_SEARCH")]
    public TileBase debugGroundTile;
    public TileBase debugEmptyTile;
    public TileBase debugFailTile;
    private Tilemap debugTilemap;
#endif
    private List<Vector3Int>[] spawnPos = new List<Vector3Int>[(int)SpawnLocation.LocationCount];
    private Tilemap tilemap;

    private void Start()
    {
        totalWaves = spawner.numberOfWaves.randomValue;

        tilemap = transform.parent.GetChild(0).GetChild(2).GetComponent<Tilemap>();
#if DEBUG_SEARCH
        GameObject debugTilemapObj = new GameObject("Debug", typeof(Tilemap), typeof(TilemapRenderer));
        debugTilemapObj.transform.parent = tilemap.transform.parent;
        debugTilemapObj.transform.localPosition = Vector3.zero;
        debugTilemap = debugTilemapObj.GetComponent<Tilemap>();
        debugTilemapObj.GetComponent<TilemapRenderer>().sortingOrder = 10;
#endif

        for (int i = 0; i < spawnPos.Length; i++)
            spawnPos[i] = new List<Vector3Int>();

        for (int y = 0; y <= tilemap.cellBounds.size.y - 3; y++)
        {
            for (int x = 0; x <= tilemap.cellBounds.size.x - 3; x++)
            {
                Vector3Int pos = tilemap.cellBounds.min + new Vector3Int(x, y, 0);
                int pattern = 0;
                for (int j = 0; j < 3; j++)
                    for (int i = 0; i < 3; i++)
                        if (tilemap.HasTile(pos + new Vector3Int(i, j, 0)))
                            pattern |= 1 << (i + j * 3);

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
        if (Enemy.numberOfEnemiesAlive <= 0 && waveCount < totalWaves)
        {
            ++waveCount;
            SpawnEnemy(spawner.numberOfEnemies.randomValue);
        }
        else if (Enemy.numberOfEnemiesAlive == 0)
        {
            totalWaves = 0;
            waveCount = 0;
            GameInput.TriggerEvent(GameEventType.EndRoom, transform.parent);
            Destroy(gameObject);
        }
    }

    // TODO: Enemies don't spawn close to doors
    private void SpawnEnemy(int enemyCount)
    {
        for (int i = 0; i <= spawner.info.Length; i++)
        {
            if (i == spawner.info.Length)
            {
                Debug.Assert(false, "All enemies are disabled");
                return;
            }
            if (spawner.info[i].enabled)
                break;
        }

        List<Vector3> usedPos = new List<Vector3>(enemyCount - 1);
        Vector3 pos;
        for (int i = 0; i < enemyCount; i++)
        {
            Optional<EnemyInfo> info = spawner.info.RandomElement();
            while (!info.enabled)
                info = spawner.info.RandomElement();
            EnemyInfo enemy = info.value;
            float enemyHeight = enemy.enemy.GetComponent<SpriteRenderer>().bounds.extents.y;

            do
            {
                Vector3 offset = new Vector3(.5f, .5f);
                if (enemy.spawnLocation == SpawnLocation.Ground)
                    offset.y = enemyHeight;
                pos = spawnPos[(int)enemy.spawnLocation].RandomElement() + tilemap.transform.position + offset;
            } while (Physics2D.BoxCast(pos, Vector2.one * 3, 0, Vector2.zero, 0) && usedPos.Contains(pos));

            usedPos.Add(pos);
            Enemy spawnedEnemy = Instantiate(enemy.enemy, pos, Quaternion.identity);
            Enemy.numberOfEnemiesAlive++;
            if (spawner.disableDropCell)
                spawnedEnemy.moneyDrop = new RangedInt(0, 0);
            if (spawner.spawnVFX.enabled)
                StartCoroutine(StartSpawnVFX(spawnedEnemy));
        }
    }

    private IEnumerator StartSpawnVFX(Enemy enemy)
    {
        float vfxTime = ObjectPooler.Spawn<Animator>(PoolType.VFX_Spawner, enemy.transform.position).GetCurrentAnimatorStateInfo(0).length;
        enemy.gameObject.SetActive(false);
        yield return new WaitForSeconds(vfxTime);
        enemy.gameObject.SetActive(true);
    }
}
