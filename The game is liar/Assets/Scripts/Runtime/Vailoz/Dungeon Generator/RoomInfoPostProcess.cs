using Edgar.Unity;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Dungeon generator/RoomInfo", fileName = "RoomInfo")]
public class RoomInfoPostProcess : DungeonGeneratorPostProcessBase
{
    public override void Run(GeneratedLevel level, LevelDescription levelDescription)
    {
        Minimap minimap = FindObjectOfType<Minimap>();
        if (minimap)
        {
            // Create new tilemap layer for the level map
            GameObject tilemapObject = new GameObject("LevelMap");
            tilemapObject.transform.SetParent(level.RootGameObject.transform.Find(GeneratorConstants.TilemapsRootName));
            tilemapObject.transform.localPosition = Vector3.zero;
            Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
            tilemapObject.AddComponent<TilemapRenderer>().sortingOrder = 20;
            tilemapObject.layer = LayerMask.NameToLayer("LevelMap");
            minimap.tilemap = tilemap;
        }
    }
}
