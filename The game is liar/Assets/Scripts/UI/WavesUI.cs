using System.Text;
using UnityEngine;
using TMPro;
using ProceduralLevelGenerator.Unity.Generators.Common.Rooms;

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
        RoomManager.instance.hasPlayer += UpdateCurrentSpawner;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (currentEnemies == Enemies.numberOfEnemiesAlive)
            return;
        StringBuilder builder = new StringBuilder();
        int numberOfWaves = startWaves - currentSpawner.numberOfWaves;
        builder.Append("Waves: ").Append(numberOfWaves).Append("/").Append(startWaves);
        numberOfWavesText.text = builder.ToString();
        builder.Clear();
        builder.Append("Number of enemies left: ").Append(Enemies.numberOfEnemiesAlive);
        numberOfEnemiesText.text = builder.ToString();
        currentEnemies = Enemies.numberOfEnemiesAlive;
        if (currentEnemies == 0 && currentSpawner.numberOfWaves <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void UpdateCurrentSpawner(RoomInstance currentRoom)
    {
        currentSpawner = currentRoom.RoomTemplateInstance.transform.Find("Enemies").GetComponent<EnemySpawner>();
        if (currentSpawner.numberOfWaves == 0)
        {
            return;
        }
        startWaves = currentSpawner.numberOfWaves;
        currentEnemies = Enemies.numberOfEnemiesAlive;
        gameObject.SetActive(true);
    }
}
