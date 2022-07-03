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
    [ContextMenu("Set this level to current")]
    // TODO: Move this function to GameManager so that other code can change current level
    void SetCurrent()
    {
        GameManager manager = FindObjectOfType<GameManager>();
        if (manager)
        {
            int i = 0;
            foreach (LevelData level in manager.levels)
            {
                if (level == this)
                    goto SUCCESS;
                else
                    ++i;
            }

            Debug.LogError("This level hasn't been added to the game manager yet!");
            return;

            SUCCESS:
            manager.levels[manager.currentLevel].gameObject.SetActive(false);
            manager.levels[i].gameObject.SetActive(true);
            manager.currentLevel = i;
        }
        else
            Debug.LogError("Can't find the game manager!");
    }
}
