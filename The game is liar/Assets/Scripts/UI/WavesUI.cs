using System.Text;
using UnityEngine;
using TMPro;

public class WavesUI : MonoBehaviour
{
    public TextMeshProUGUI numberOfWavesText;
    public TextMeshProUGUI numberOfEnemiesText;
    private int startWaves, currentEnemies;
    private EnemySpawner currentSpawner;
    public static WavesUI instance;

    void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentEnemies == EnemySpawner.numberOfEnemiesAlive)
            return;
        StringBuilder builder = new StringBuilder();
        int numberOfWaves = startWaves - currentSpawner.numberOfWaves;
        builder.Append("Waves: ").Append(numberOfWaves).Append("/").Append(startWaves);
        numberOfWavesText.text = builder.ToString();
        builder.Clear();
        builder.Append("Number of enemies left: ").Append(EnemySpawner.numberOfEnemiesAlive);
        numberOfEnemiesText.text = builder.ToString();
        currentEnemies = EnemySpawner.numberOfEnemiesAlive;
        if (currentEnemies == 0 && currentSpawner.numberOfWaves <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void UpdateCurrentSpawner(ProceduralLevelGenerator.Unity.Generators.Common.Rooms.RoomInstance currentRoom)
    {
        currentSpawner = currentRoom.RoomTemplateInstance.transform.Find("Enemies").GetComponent<EnemySpawner>();
        if (currentSpawner.numberOfWaves == 0)
        {
            return;
        }
        startWaves = currentSpawner.numberOfWaves;
        currentEnemies = EnemySpawner.numberOfEnemiesAlive;
        gameObject.SetActive(true);
    }
}
