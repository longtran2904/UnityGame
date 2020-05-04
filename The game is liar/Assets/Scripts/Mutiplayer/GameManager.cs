using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region UI Variables
    public static GameManager instance;
    public GameObject loadingScreen;
    public ProgressBar bar;
    #endregion

    //#region Multiplayer Variables
    //public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    //public GameObject localPlayerPrefab;
    //public GameObject playerPrefab;
    //#endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, Destroying object!");
            Destroy(this);
        }

        SceneManager.LoadSceneAsync((int)SceneIndexes.START_MENU, LoadSceneMode.Additive);
    }

    #region Scene Handler
    List<AsyncOperation> scenesLoading = new List<AsyncOperation>();

    public void LoadGame(int sceneIndex, bool isAdditive = false)
    {
        loadingScreen.gameObject.SetActive(true);

        scenesLoading.Add(SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex));

        if (isAdditive)
        {
            scenesLoading.Add(SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive));
        }
        else
        {
            scenesLoading.Add(SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single));
        }        

        StartCoroutine(GetSceneLoadProgress(sceneIndex));
    }

    public void LoadGame()
    {
        loadingScreen.gameObject.SetActive(true);
        scenesLoading.Add(SceneManager.UnloadSceneAsync((int)SceneIndexes.START_MENU));
        scenesLoading.Add(SceneManager.LoadSceneAsync((int)SceneIndexes.CART, LoadSceneMode.Additive));

        StartCoroutine(GetSceneLoadProgress());
    }

    float totalSceneProgress;

    public IEnumerator GetSceneLoadProgress(int _sceneIndex = 2)
    {
        for (int i = 0; i < scenesLoading.Count; i++)
        {
            while (!scenesLoading[i].isDone)
            {
                totalSceneProgress = 0;

                foreach (AsyncOperation operation in scenesLoading)
                {
                    totalSceneProgress += operation.progress;
                }

                totalSceneProgress = (totalSceneProgress / scenesLoading.Count) * 100;

                if (bar)
                {
                    bar.current = Mathf.RoundToInt(totalSceneProgress);
                }

                yield return null;
            }
        }

        loadingScreen.gameObject.SetActive(false);

        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(_sceneIndex));
    }
    #endregion

    //#region Handle Multiplayer
    //public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
    //{
    //    GameObject _player;

    //    if (_id == Client.instance.myId)
    //    {
    //        _player = Instantiate(localPlayerPrefab, _position, _rotation);
    //    }
    //    else
    //    {
    //        _player = Instantiate(playerPrefab, _position, _rotation);
    //    }

    //    _player.GetComponent<PlayerManager>().id = _id;
    //    _player.GetComponent<PlayerManager>().username = _username;
    //    players.Add(_id, _player.GetComponent<PlayerManager>());
    //}
    //#endregion
}
