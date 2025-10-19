using UnityEngine;
using UnityEngine.Tilemaps;

public enum BoundsType
{
    None,
    Generator,
    Tilemap,
    Custom,
}

public class LevelData : MonoBehaviour
{
    [Header("Camera")]
    public BoundsType type;

    [ShowWhen("type", new object[] { BoundsType.Generator, BoundsType.Tilemap, BoundsType.Custom })]
    public bool moveAutomatically;
    [ShowWhen("type", new object[] { BoundsType.Generator, BoundsType.Tilemap, BoundsType.Custom })]
    public bool useSmoothDamp;
    [ShowWhen("type", new object[] { BoundsType.Generator, BoundsType.Tilemap, BoundsType.Custom })]
    public Vector2 cameraValue;
    [ShowWhen("moveAutomatically")] public float waitTime;

    [ShowWhen("type", BoundsType.Generator)] public Edgar.Unity.DungeonGenerator generator;
    [ShowWhen("type", BoundsType.Generator)] public int maxGenerateTry;
    [ShowWhen("type", BoundsType.Generator)] public bool disableEnemies;

    [ShowWhen("type", BoundsType.Tilemap)] public Tilemap tilemap;

    [ShowWhen("type", BoundsType.Custom)] public Vector2 boundsSize;

    [Header("UI")]
    public bool enableUI;
    [ShowWhen("enableUI")] public bool enableMinimap;

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [EasyButtons.Button]
    // TODO: Move this function to GameManager so that other code can change current level
    void SetCurrent()
    {
        GameManager manager = FindObjectOfType<GameManager>();
        GameDebug.Assert(manager, "Can't find the game manager", () =>
        {
            int i = 0;
            foreach (LevelData level in manager.levels)
            {
                if (level == this)
                    manager.currentLevel = i;
                level.gameObject.SetActive(false);
                ++i;
            }

            if (manager.levels[manager.currentLevel] != this)
                Debug.LogWarning("This level hasn't been added to the game manager yet!"); // TODO: Just add the level if it's not already in the list
            manager.levels[manager.currentLevel].gameObject.SetActive(true);
        });
    }
}
