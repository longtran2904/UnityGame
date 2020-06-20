using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySpawner : MonoBehaviour
{
    public int numberOfWaves;

    public int minEnemiesToSpawn;

    public int maxEnemiesToSpawn;

    private int numberOfEnemiesToSpawn;

    public Transform[] spawnPoints;

    public EnemiesProb[] enemies;

    public bool active = false;

    public bool teleportToNextScene = false;

    private int numberOfEnemiesAlive = 0;

    private List<GameObject> enemiesObject = new List<GameObject>();

    Player player;

    bool hasPlay = false;

    private void Start()
    {
        if (teleportToNextScene)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            player.tpDelegate += Teleport;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!active)
        {
            return;
        }

        if (enemiesObject != null)
        {
            for (int i = enemiesObject.Count - 1; i >= 0; i--)
            {
                if (enemiesObject[i] == null)
                {
                    numberOfEnemiesAlive--;
                    enemiesObject.RemoveAt(i);
                }
            }
        }
        else
        {
            numberOfEnemiesAlive = 0;
        }

        if (numberOfEnemiesAlive <= 0 && numberOfWaves > 0)
        {
            numberOfEnemiesAlive = 0;

            numberOfEnemiesToSpawn = Random.Range(minEnemiesToSpawn, maxEnemiesToSpawn + 1);

            SpawnEnemy(numberOfEnemiesToSpawn);

            numberOfWaves--;
        }

        if (teleportToNextScene && numberOfWaves <= 0 && numberOfEnemiesAlive <= 0)
        {
            if (hasPlay)
            {
                return;
            }
            player.PlayTeleportAnimation();
            hasPlay = true;
        }
    }

    void Teleport()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().tpDelegate -= Teleport;
        GameManager.instance.LoadGame(SceneManager.GetActiveScene().buildIndex + 1, true);
    }

    public void SpawnEnemy(int _numberOfEnemiesToSpawn)
    {
        numberOfEnemiesAlive += numberOfEnemiesToSpawn;

        if (spawnPoints == null || enemies == null)
        {
            return;
        }

        if (_numberOfEnemiesToSpawn > spawnPoints.Length)
        {
            Debug.LogWarning("Number of enemies to spawn is greater than spawn points");
            return;
        }

        float[] probs = new float[enemies.Length];

        for (int i = 0; i < enemies.Length; i++)
        {
            probs[i] = enemies[i].prob;
        }

        Transform[] spawnPos = ChooseSet(_numberOfEnemiesToSpawn);

        foreach (Transform spawnPoint in spawnPos)
        {
            enemiesObject.Add(Instantiate(enemies[(int)Choose(probs)].enemyPrefab, spawnPoint.position, Quaternion.identity));
        }
    }

    Transform[] ChooseSet (int numRequired) {
        Transform[] result = new Transform[numRequired];

        int numToChoose = numRequired;

        for (int numLeft = spawnPoints.Length; numLeft > 0; numLeft--) {

            float prob = (float)numToChoose/(float)numLeft;

            if (Random.value <= prob) {
                numToChoose--;
                result[numToChoose] = spawnPoints[numLeft - 1];

                if (numToChoose == 0) {
                    break;
                }
            }
        }
        return result;
    }

    float Choose(float[] probs)
    {

        float total = 0;

        foreach (float elem in probs)
        {
            total += elem;
        }

        float randomPoint = Random.value * total;

        for (int i = 0; i < probs.Length; i++)
        {
            if (randomPoint < probs[i])
            {
                return i;
            }
            else
            {
                randomPoint -= probs[i];
            }
        }
        return probs.Length - 1;
    }
}
