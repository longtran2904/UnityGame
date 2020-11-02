using UnityEngine;

public class MainMenu : MonoBehaviour
{
    void Awake()
    {
        AudioManager.instance.StopAll();
        AudioManager.instance.Play("8bit");
    }

    public void PlayGame()
    {
        AudioManager.instance.Play("Select");
        GameManager.instance.LoadGame((int)SceneIndexes.ROOM, true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
