using UnityEngine;

[CreateAssetMenu(fileName = "Game Settings", menuName = "Data/Game Settings")]
public class GameSettings : ScriptableObject
{
    public const int fullScreenModeCount = 4;

    [Header("Video Settings")]
    public FullScreenMode mode;
    public Resolution resolution;
    public bool vsync;
    // public float gamma;
    // public ScailingMode scailingMode;
    // public QualityMode qualityMode;
    // public PostProcessingMode postProcessingMode;
    // public LightShadowMode lightShadowMode;

    public void Copy(GameSettings newSettings)
    {
        mode = newSettings.mode;
        resolution = newSettings.resolution;
        vsync = newSettings.vsync;
    }

    public static bool CompareSettings(GameSettings a, GameSettings b)
    {
        if (a == b)
            return true;
        else
            return a.mode == b.mode && CompareResolution(a.resolution, b.resolution) && a.vsync == b.vsync;
    }

    public static bool CompareResolution(Resolution a, Resolution b)
    {
        return a.width == b.width && a.height == b.height;
    }
}
