using System.IO;
using UnityEngine;
using ProceduralLevelGenerator.Unity.Generators.Common.RoomTemplates.RoomTemplateInitializers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Scripts
{
    public class CustomRoomTemplateInitializer : RoomTemplateInitializerBase
    {
        public override void Initialize()
        {
            base.Initialize();

            // Here is where i place my custom gameobject
            var cameraInfoObject = CreateCustomGameObject<CameraInfo>("Camera Info", gameObject);
            var lightsObject = CreateCustomGameObject("Lights", gameObject);
            var enemiesObject = CreateCustomGameObject<EnemySpawner>("Enemies", gameObject);
        }

        // Here is a function i made to create custom gameobject with different components
        protected GameObject CreateCustomGameObject<T>(string name, GameObject parentObject) where T : Component
        {
            var customObject = new GameObject(name);
            customObject.transform.SetParent(parentObject.transform);
            var customComponent = customObject.AddComponent<T>();

            return customObject;
        }

        protected GameObject CreateCustomGameObject(string name, GameObject parentObject)
        {
            var tilemapObject = new GameObject(name);
            tilemapObject.transform.SetParent(parentObject.transform);

            return tilemapObject;
        }

        protected override void InitializeTilemaps(GameObject tilemapsRoot)
        {
            // Create an instance of your tilemap layers handler
            var tilemapLayersHandler = ScriptableObject.CreateInstance<CustomTilemapsLayersHandler>();

            // Initialize tilemaps
            tilemapLayersHandler.InitializeTilemaps(tilemapsRoot);
        }

        // Add create menu field to create a room template with this room template initializer
        // This should be simplified in the next version of the plugin so that users do not have to copy paste this
#if UNITY_EDITOR
        [MenuItem("Assets/Create/Dungeon generator/Custom room template")]
        public static void CreatePlatformerRoomTemplate()
        {
            // Create empty game object
            var roomTemplate = new GameObject();

            // Add room template initializer, initialize room template, destroy initializer
            // ONLY THIS LINE HAS TO BE CHANGED if we want to add a menu item with a different RoomTemplateInitializer
            var roomTemplateInitializer = roomTemplate.AddComponent<CustomRoomTemplateInitializer>();
            roomTemplateInitializer.Initialize();
            Object.DestroyImmediate(roomTemplateInitializer);

            // Save prefab
            var currentPath = GetCurrentPath();
            PrefabUtility.SaveAsPrefabAsset(roomTemplate, AssetDatabase.GenerateUniqueAssetPath(currentPath + "/Custom Room template.prefab"));

            // Remove game object from scene
            Object.DestroyImmediate(roomTemplate);
        }

        public static string GetCurrentPath()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            return path;
        }
#endif
    }
}