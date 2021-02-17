using UnityEngine;

public class MainMenu : MonoBehaviour
{
    void Awake()
    {
        AudioManager.instance.StopMusic();
        AudioManager.instance.PlaySfx("8bit");
    }

    public void PlayGame()
    {
        AudioManager.instance.PlaySfx("Select");
        GameManager.instance.LoadGame((int)SceneIndexes.ROOM, true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
