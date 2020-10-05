using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool isGamePaused = false;
    private bool inOptionMenu;

    public GameObject pauseMenuUI;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !inOptionMenu)
        {
            if (isGamePaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1;
        isGamePaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0;
        isGamePaused = true;
    }

    public void OnOptionMenu()
    {
        inOptionMenu = true;
    }

    public void OffOptionMenu()
    {
        inOptionMenu = false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ExitToMainMenu()
    {
        GameManager.instance.LoadGame((int)SceneIndexes.START_MENU, true);
    }
}
