using ProceduralLevelGenerator.Unity.Generators.Common.RoomTemplates.TilemapLayers;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts
{
    // TilemapLayersHandlerBase inherit from ScriptableObject so we need to create an asset menu field that we will use to create the scriptable object instance
    // The menu name can be changed to anything you want
    [CreateAssetMenu(menuName = "Dungeon generator/Custom tilemap layers handler", fileName = "CustomTilemapLayersHandler")]
    public class CustomTilemapsLayersHandler : TilemapLayersHandlerBase
    {
        public Material material;

        public override void InitializeTilemaps(GameObject gameObject)
        {
            // First make sure that you add the grid component
            gameObject.AddComponent<Grid>();

            // And then create child game objects with their tilemaps
            var background1TilemapObject = CreateTilemapGameObject("Background 1", gameObject, 0);

            var background2TilemapObject = CreateTilemapGameObject("Background 2", gameObject, 1);

            var wallsTilemapObject = CreateTilemapGameObject("Walls", gameObject, 2);
            AddCollider(wallsTilemapObject);
            wallsTilemapObject.tag = "Ground";
            wallsTilemapObject.layer = LayerMask.NameToLayer("Ground");

            var collideableTilemapObject = CreateTilemapGameObject("Collideable", gameObject, 3);
            AddCollider(collideableTilemapObject);

            var other2TilemapObject = CreateTilemapGameObject("Other 1", gameObject, 4);

            var other3TilemapObject = CreateTilemapGameObject("Other 2", gameObject, 5);
        }

        protected GameObject CreateTilemapGameObject(string name, GameObject parentObject, int sortingOrder, string sortingLayerName = "Default")
        {
            var tilemapObject = new GameObject(name);
            tilemapObject.transform.SetParent(parentObject.transform);
            var tilemap = tilemapObject.AddComponent<Tilemap>();
            var tilemapRenderer = tilemapObject.AddComponent<TilemapRenderer>();
            tilemapRenderer.sortingOrder = sortingOrder;
            tilemapRenderer.sortingLayerName = sortingLayerName;
            if (material)
            {
                tilemapRenderer.sharedMaterial = material;
            }
            return tilemapObject;
        }

        protected void AddCollider(GameObject gameObject, bool isTrigger = false)
        {
            var tilemapCollider2D = gameObject.AddComponent<TilemapCollider2D>();
            tilemapCollider2D.isTrigger = isTrigger;
        }
    }
}