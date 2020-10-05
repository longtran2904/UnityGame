using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Awake()
    {
        AudioManager.instance.StopAll();
        AudioManager.instance.Play("8bit");
    }

    public void PlayGame()
    {
        GameManager.instance.LoadGame((int)SceneIndexes.ROOM, true);
        AudioManager.instance.Play("Select");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
