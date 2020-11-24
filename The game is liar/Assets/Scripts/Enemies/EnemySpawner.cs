using UnityEngine.Events;
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
    public bool teleportToNextScene = false;
    public UnityEvent endWaves;

    Player player;
    bool hasPlay = false;

    private void Start()
    {
        // For cart scene
        if (teleportToNextScene)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            player.teleportEvent += Teleport;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Enemies.numberOfEnemiesAlive <= 0 && numberOfWaves > 0)
        {
            Enemies.numberOfEnemiesAlive = 0;
            numberOfEnemiesToSpawn = Random.Range(minEnemiesToSpawn, maxEnemiesToSpawn + 1);
            SpawnEnemy(numberOfEnemiesToSpawn);
            numberOfWaves--;
        }
        else if (Enemies.numberOfEnemiesAlive == 0)
        {
            endWaves.Invoke();
            Destroy(gameObject);
        }

        // For cart scene
        if (teleportToNextScene && numberOfWaves <= 0 && Enemies.numberOfEnemiesAlive <= 0)
        {
            if (hasPlay)
            {
                return;
            }
            player.PlayTeleportAnimation();
            hasPlay = true;
        }
    }

    // For cart scene
    void Teleport()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().teleportEvent -= Teleport;
        GameManager.instance.LoadGame(SceneManager.GetActiveScene().buildIndex + 1, true);
    }

    public void SpawnEnemy(int _numberOfEnemiesToSpawn)
    {
        if (spawnPoints == null || enemies == null)
        {
            return;
        }
        if (_numberOfEnemiesToSpawn > spawnPoints.Length)
        {
            InternalDebug.LogError("Number of enemies to spawn is greater than spawn points: " + transform.parent.gameObject.name + " InstanceID: " + transform.GetInstanceID());
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
            Instantiate(enemies[(int)Choose(probs)].enemyPrefab, spawnPoint.position, Quaternion.identity);
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
