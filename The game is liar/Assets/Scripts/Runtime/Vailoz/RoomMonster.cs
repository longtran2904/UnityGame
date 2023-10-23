#define DEBUG_SEARCH

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum RegionType
{
    None,
    Air,
    Ground,
    Platform,
    Walk,
    
    Count
}

public class RoomMonster : MonoBehaviour
{
    [MinMax(0, 10)] public RangedInt numberOfEnemies;
    public float minDistance;
    public Vector2 searchRange;
    public float minRegionWidth;
    public bool removeOneTile;
    public float minDegree;
    
    public int maxIteration = 100;
    public int maxTryPerIteration = 10;
    
    private List<Vector2>[] spawnRegions;
    private List<Rect> moveRegions;
    private List<Vector2> spawnPositions;
    private List<Rect> nextRegions;
    
    [Header("Debug")]
    public Tilemap tilemap;
    public Tile[] debugTiles;
    
    public int currentMoveRegion;
    public PositionType position;
    
    public bool stopWhenHitError;
    private bool error;
    
    public string debugName = "Test Spawner";
    
    public enum DebugSearchType
    {
        WalkRegion,
        PlayerRegion,
        NextRegion,
        SearchRegion,
        PossibleTile,
        SpawnPosition,
    }
    public Property<DebugSearchType> enables;
    
    public Color walkRegionColor = Color.green;
    public Color playerPosColor = Color.red;
    public Color currentRectColor = Color.magenta;
    public Color nextRegionColor = Color.red;
    public Color searchRegionColor = Color.cyan;
    public Color[] spawnColors = new Color[] { Color.yellow, Color.green, Color.cyan, Color.magenta, Color.red, Color.white, Color.gray, Color.black };
    
    public void Init()
    {
#if DEBUG_SEARCH
        string objName = "Debug";
        GameObject debugTilemapObj = transform.Find(objName)?.gameObject ?? new GameObject(objName, typeof(Tilemap), typeof(TilemapRenderer));
        debugTilemapObj.transform.parent = transform;
        debugTilemapObj.transform.localPosition = Vector3.zero;
        debugTilemapObj.GetComponent<TilemapRenderer>().sortingOrder = 10;
        Tilemap debugTilemap = debugTilemapObj.GetComponent<Tilemap>().ClearAndCompress();
#endif
        
        moveRegions = new List<Rect>(8);
        spawnRegions = new List<Vector2>[(int)RegionType.Count];
        for (int i = 0; i < spawnRegions.Length; i++)
            spawnRegions[i] = new List<Vector2>(64);
        
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
                pos += Vector2Int.one.ToVector3Int(); // NOTE: we're looping from the bottom left tile but checking the middle tile
                
                if (((pattern & 0b000010000) != 0) &&
                    (((pattern & 0b010000000) == 0) || ((pattern & 0b000000010) == 0)))
                    AddPattern(RegionType.Walk);
                
                if (pattern == 0b000111000) // NOTE: platform tiles may have special tiles that I need to check
                    AddPattern(RegionType.Platform);
                else if (pattern == 0b111111000 || pattern == 0b000111111)
                    AddPattern(RegionType.Ground);
                else if (pattern == 0)
                    AddPattern(RegionType.Air);
                
                void AddPattern(RegionType type)
                {
                    spawnRegions[(int)type].Add((Vector2Int)pos);
#if DEBUG_SEARCH
                    if (type != RegionType.Walk)
                        debugTilemap.SetTile(pos, debugTiles[(int)type]);
#endif
                }
            }
        }
        
        {
            List<Vector2> walkTiles = spawnRegions[(int)RegionType.Walk];
            Rect currentWalkRegion = new Rect();
            int i = 0;
            while (i < walkTiles.Count)
            {
                currentWalkRegion = new Rect(walkTiles[i], Vector2.up);
                do
                {
                    currentWalkRegion.width++;
                    i++;
                } while ((i != walkTiles.Count) && currentWalkRegion.ContainsEx(walkTiles[i], true));
                moveRegions.Add(currentWalkRegion);
            }
        }
        
        if (removeOneTile)
        {
            List<Vector2> airTiles = spawnRegions[(int)RegionType.Air];
            for (int i = airTiles.Count - 1; i >= 0; --i)
            {
                bool top = debugTilemap.GetTile((airTiles[i] + new Vector2(0, 1)).ToVector2Int().ToVector3Int());
                bool left = debugTilemap.GetTile((airTiles[i] + new Vector2(-1, 0)).ToVector2Int().ToVector3Int());
                bool right = debugTilemap.GetTile((airTiles[i] + new Vector2(1, 0)).ToVector2Int().ToVector3Int());
                bool bot = debugTilemap.GetTile((airTiles[i] + new Vector2(0, -1)).ToVector2Int().ToVector3Int());
                if ((!top && !bot) || (!left && !right))
                {
                    debugTilemap.SetTile(airTiles[i].ToVector2Int().ToVector3Int(), null);
                    airTiles.RemoveAt(i);
                }
            }
        }
        
        spawnPositions = new List<Vector2>(numberOfEnemies.max);
        nextRegions = new List<Rect>(moveRegions.Count);
    }
    
    private void OnDrawGizmos()
    {
        GameDebug.RenderDiagram(debugName, true, enables.properties);
    }
    
    
    [EasyButtons.Button(Spacing = EasyButtons.ButtonSpacing.Inline)]
    public void ResetCurrentRegion() => currentMoveRegion = 0;
    [EasyButtons.Button(Spacing = EasyButtons.ButtonSpacing.Inline)]
    public void SwitchPositionMode() => position = (PositionType)MathUtils.LoopIndex((int)position + 1,
                                                                                     System.Enum.GetNames(typeof(PositionType)).Length, true);
    
    [EasyButtons.Button(Spacing = EasyButtons.ButtonSpacing.Before)]
    public void SpawnRandomRegion() => TestSpawn(-2);
    
    [EasyButtons.Button(Spacing = EasyButtons.ButtonSpacing.Inline)]
    public void SpawnPrevRegion() => TestSpawn(-1);
    [EasyButtons.Button(Spacing = EasyButtons.ButtonSpacing.Inline)]
    public void SpawnCurrentRegion() => TestSpawn(0);
    [EasyButtons.Button(Spacing = EasyButtons.ButtonSpacing.Inline)]
    public void SpawnNextRegion() => TestSpawn(1);
    
    [EasyButtons.Button]
    public void ClearSpawn()
    {
        spawnPositions?.Clear();
        nextRegions?.Clear();
        
        GameDebug.ClearDiagram(debugName);
        GameDebug.ClearLog();
        UnityEditor.SceneView.RepaintAll();
        error = false;
    }
    
    [EasyButtons.Button]
    public void SpawnTest(int iteration)
    {
        ClearSpawn();
        Init();
        ResetCurrentRegion();
        
        position = PositionType.Random;
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        for (int i = 0; i < moveRegions.Count; i++)
        {
            for (int j = 0; j < iteration; j++)
                SpawnCurrentRegion();
            if (i < moveRegions.Count - 1)
                SpawnNextRegion();
            
            if (error)
            {
                Debug.Log("Index: " + i);
                break;
            }
        }
        watch.Stop();
        Debug.Log("Error: " + error + " Elapsed: " + watch.Elapsed);
    }
    
    private void TestSpawn(int offsetIndex)
    {
        if (stopWhenHitError && error)
        {
            Debug.Log("Hit error");
            return;
        }
        
        GameDebug.BeginDebug(debugName, true, true, assert: success => { if (!error) error = !success; });
        ClearSpawn();
        //if (moveRegions == null || spawnRegions == null || moveRegions.Count == 0)
        Init();
        
        Vector2 playerSize = new Vector2(1, 1);
        int dir = MathUtils.Random() ? 1 : -1;
        
        Vector2 playerPos = new Vector2();
        {
            if (offsetIndex == -2)
                currentMoveRegion = Random.Range(0, moveRegions.Count);
            else
                currentMoveRegion = MathUtils.LoopIndex(currentMoveRegion + offsetIndex, moveRegions.Count, true);
            Rect region = moveRegions[currentMoveRegion];
            switch (position)
            {
                case PositionType.Random:
                {
                    playerPos = region.RandomPoint();
                    playerPos.x = Mathf.Clamp(playerPos.x, region.xMin, region.xMax - 1);
                } break;
                case PositionType.Min:
                {
                    playerPos = region.min;
                } break;
                case PositionType.Max:
                {
                    playerPos = region.max + Vector2.left;
                } break;
                case PositionType.Middle:
                {
                    playerPos = region.center;
                } break;
            }
            playerPos.y = region.y;
        }
        
        {
            if (moveRegions[moveRegions.Count - 1].y == playerPos.y)
                dir = -1;
            else if (moveRegions[0].y == playerPos.y)
                dir = 1;
            playerPos += new Vector2(.5f, .5f);
            playerPos.y += playerSize.y * dir;
        }
        
        Rect playerRegion = MathUtils.CreateRect(playerPos, playerSize);
        GameDebug.DiagramBox(playerRegion, playerPosColor, (int)DebugSearchType.PlayerRegion);
        GameDebug.DiagramBoxes(moveRegions, walkRegionColor, (int)DebugSearchType.WalkRegion);
        
        SpawnMonsters(playerPos, playerSize, dir);
        GameDebug.DiagramConnections(playerRegion, nextRegions, playerPosColor, nextRegionColor, Color.white, (int)DebugSearchType.NextRegion, false);
        
        GameDebug.EndDebug();
        UnityEditor.SceneView.RepaintAll();
    }
    
    public void SpawnMonsters(Vector2 playerPos, Vector2 playerSize, int dir)
    {
        GetNextMoveRegion(playerPos, playerSize, searchRange, minRegionWidth, dir, moveRegions, nextRegions);
        int enemyCount = numberOfEnemies.randomValue;
        spawnPositions.Clear();
        List<List<Vector2>> debugSpawnRegions = new List<List<Vector2>>(enemyCount);
        
        for (int i = 0; i < enemyCount; ++i)
        {
            Vector2 spawnPos = Vector2.zero;
            for (int j = 0; j < maxIteration; ++j)
            {
                List<Vector2> spawnRegion = GetPossibleSpawnRegion(playerPos, nextRegions, spawnPositions, spawnRegions, minDistance);
                int tryPerIteration = 0;
                bool success = false;
                do
                {
                    tryPerIteration++;
                    if (tryPerIteration == maxTryPerIteration || spawnRegion.Count == 0)
                        break;
                    spawnPos = spawnRegion.RandomElement();
                } while (!(success = IsValid(spawnPos, playerPos, spawnPositions, minDistance)));
                
                if (success)
                {
                    GameDebug.DiagramTiles(spawnRegion, spawnColors[i], Vector2.one * Mathf.Lerp(.5f, .9f, (float)i / enemyCount), (int)DebugSearchType.PossibleTile);
                    GameDebug.DiagramBox(MathUtils.CreateRect(spawnPos, Vector2.one, 1.25f), spawnColors[i], (int)DebugSearchType.SpawnPosition);
                    debugSpawnRegions.Add(spawnRegion);
                    spawnPositions.Add(spawnPos);
                    break;
                }
                
                if (j == maxIteration - 1)
                    GameDebug.Log($"Can't find any valid positions. Index: {i}, Enemy: {enemyCount}, Regions: {spawnRegion.Count}", true, LogType.Error);
            }
        }
        
        GameDebug.Log("Enemy: " + enemyCount + " Regions: " + debugSpawnRegions.Count + " Positions: " + spawnPositions.Count + " Next regions: " + nextRegions.Count);
        GameDebug.Log(GameUtils.GetAllString(debugSpawnRegions, "Spawn Regions: " , lineWidth: 8, toString: (x, _) => x.Count.ToString()));
        GameDebug.Log(GameUtils.GetAllString(spawnPositions, "Spawn Positions: "  , lineWidth: 8));
        GameDebug.Log(GameUtils.GetAllString(nextRegions, "Next Walk Regions: "   , lineWidth: 5));
        
        foreach (Vector2 pos in spawnPositions)
        {
            // TODO: Spawn enemies
        }
    }
    
    public void GetNextMoveRegion(Vector2 playerPos, Vector2 playerSize, Vector2 range, float minSize, int dir, List<Rect> moveRegions, List<Rect> result)
    {
        GameDebug.Assert(dir == 1 || dir == -1, "Invalid dir passed in: " + dir);
        
        Rect current = default;
        Rect searchRect = MathUtils.CreateRect(playerPos, range * 2);
        GameDebug.DiagramBox(searchRect, searchRegionColor, (int)DebugSearchType.SearchRegion);
        result.Clear();
        
        Vector2 playerOffsetY = Vector2.down * (playerSize.y / 2 + .5f) * dir;
        
        Vector2 offsetPos = playerPos + playerOffsetY;
        foreach (Rect region in moveRegions)
        {
            GameDebug.Assert(region.height == 1, "Failed region: " + region);
            
            if (region.Contains(offsetPos))
            {
                GameDebug.Assert(current == default, current);
                current = region;
                GameDebug.DiagramBox(current, currentRectColor, (int)DebugSearchType.PlayerRegion);
            }
            
            if (region.Overlaps(searchRect))
                result.Add(region);
        }
        GameDebug.Assert(current != default, offsetPos);
        
        // TODO: Handle one way region and surrounding walls
        for (int i = result.Count - 1; i >= 0; i--)
        {
            if (result[i].y == current.y)
                goto REMOVE;
            
            Rect moveRect = MathUtils.GetOverlap(result[i], searchRect);
            moveRect.y = result[i].y;
            if (moveRect.width < minSize && moveRect.width < result[i].width)
            {
                float width = Mathf.Min(minSize, result[i].width);
                if (moveRect.xMax == result[i].xMax)
                    moveRect.xMin -= width - moveRect.width;
                moveRect.width = width;
            }
            
            Vector2 offsetY = playerOffsetY;
            if (Mathf.Sign(current.y - moveRect.y) == dir)
            {
                if (moveRect.min.x > current.min.x && moveRect.max.x < current.max.x)
                    goto REMOVE;
                offsetY *= -1;
            }
            
            Vector2 playerOffsetX = new Vector2(playerSize.x / 2, 0);
            moveRect = moveRect.Resize(playerOffsetX + offsetY, -playerOffsetX + offsetY);
            moveRect.height = playerSize.y;
            //moveRect.height = 0;
            
            result[i] = moveRect;
            continue;
            
            REMOVE:
            result.RemoveAt(i);
        }
        
        for (int i = result.Count - 1; i >= 0; i--)
        {
            bool drawTo;
            Rect toRect;
            
            if (!MathUtils.RangeInRange(result[i].xMin, result[i].width, current.xMin, current.xMin))
            {
                Vector2 resultClosestPoint = new Vector2(result[i].xMax, result[i].y);
                Vector2 currentClosestPoint = new Vector2(current.xMin, current.y - playerOffsetY.y);
                if (result[i].xMin > current.xMax)
                {
                    resultClosestPoint.x = result[i].xMin;
                    currentClosestPoint.x = current.xMax;
                }
                
                drawTo = false;
                toRect = new Rect(currentClosestPoint, playerSize);
                
                if (Mathf.Abs(Vector2.Dot((resultClosestPoint - currentClosestPoint).normalized, Vector2.up)) < minDegree)
                    goto REMOVE;
            }
            
            foreach (Rect region in moveRegions)
            {
                if (MathUtils.Sign(region.y - current.y) == MathUtils.Sign(result[i].y - region.y))
                {
                    drawTo = true;
                    toRect = region;
                    
                    // NOTE: Currently, we only check for whether the x is overlapping or not, not the angle forms from the current rect.
                    // So there're cases where there is another rect in-between, but the player can still get there.
                    if (region.xMin <= result[i].xMin && region.xMax >= result[i].xMax)
                        goto REMOVE;
                }
            }
            
            continue;
            REMOVE:
            GameDebug.DiagramConnection(result[i], toRect, Color.white, Color.cyan, Color.white, (int)DebugSearchType.NextRegion, drawB: drawTo);
            result.RemoveAt(i);
        }
    }
    
    public static List<Vector2> GetPossibleSpawnRegion(Vector2 playerPos, List<Rect> nextRegions, List<Vector2> spawnPos, List<Vector2>[] spawnRegions, float minRange)
    {
        List<Vector2>[] result = new List<Vector2>[spawnRegions.Length - 2]; // -2 for Walk and None
        GameDebug.Assert(spawnRegions.Length == (int)RegionType.Count, spawnRegions.Length);
        GameDebug.Assert(RegionType.Walk == RegionType.Count - 1, RegionType.Walk);
        for (int n = 1; n < (int)RegionType.Walk; ++n)
        {
            List<Vector2> validSpawnRegions = new List<Vector2>(spawnRegions[n]);
            Vector2 range = new Vector2(minRange, minRange);
            for (int i = validSpawnRegions.Count - 1; i >= 0; i--)
            {
                if (MathUtils.InRange(validSpawnRegions[i], playerPos, minRange))
                    goto END;
                
                foreach (Vector2 walkTile in spawnRegions[(int)RegionType.Walk])
                    if (validSpawnRegions[i] == walkTile)
                    goto END;
                
                foreach (Vector2 spawn in spawnPos)
                    if (MathUtils.InRange(validSpawnRegions[i], spawn, minRange))
                    goto END;
                
                for (int j = 0; j < nextRegions.Count; ++j)
                    if (nextRegions[j].Resize(-range, range).Contains(validSpawnRegions[i]))
                    goto END; // TODO: Rather remove all the positions, have some heuristics that an enemy can spawner near the next move region.
                
                continue;
                END:
                validSpawnRegions.RemoveAt(i);
            }
            result[n - 1] = validSpawnRegions;
        }
        
        // TODO: replace randomness with some actual heuristic that use the already laid-out positions
        return result.RandomElement();
    }
    
    public static bool IsValid(Vector2 pos, Vector2 playerPos, List<Vector2> spawnPos, float minRange)
    {
        if (MathUtils.InRange(pos, playerPos, minRange))
            return false;
        foreach (Vector2 p in spawnPos)
            if (MathUtils.InRange(pos, p, minRange))
            return false;
        return true;
    }
}
