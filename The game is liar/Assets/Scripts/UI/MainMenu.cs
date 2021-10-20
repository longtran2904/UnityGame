using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public AudioManager audioManager;

    void Awake()
    {
        audioManager.StopMusic();
        audioManager.PlaySfx("8bit");
    }

    public void PlayGame()
    {
        audioManager.PlaySfx("Select");
        GameManager.instance.LoadGame((int)SceneIndexes.ROOM, true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
