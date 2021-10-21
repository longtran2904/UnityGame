using System.Text;
using UnityEngine;
using TMPro;

public class WavesUI : MonoBehaviour
{
    public TextMeshProUGUI numberOfWavesText;
    public TextMeshProUGUI numberOfEnemiesText;
    private int startWaves, currentEnemies;
    private EnemySpawner currentSpawner;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (currentEnemies == Enemy.numberOfEnemiesAlive)
            return;
        if (!currentSpawner)
        {
            gameObject.SetActive(false);
            return;
        }
        StringBuilder builder = new StringBuilder();
        int numberOfWaves = startWaves - currentSpawner.waveCount;
        builder.Append("Waves: ").Append(numberOfWaves).Append("/").Append(startWaves);
        numberOfWavesText.text = builder.ToString();
        builder.Clear();
        builder.Append("Number of enemies left: ").Append(Enemy.numberOfEnemiesAlive);
        numberOfEnemiesText.text = builder.ToString();
        currentEnemies = Enemy.numberOfEnemiesAlive;
        if (currentEnemies == 0 && currentSpawner.waveCount <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void UpdateCurrentSpawner(RoomInstanceVariable currentRoom)
    {
        currentSpawner = currentRoom.value.RoomTemplateInstance.transform.Find("Enemies")?.GetComponent<EnemySpawner>();
        if (currentSpawner) startWaves = currentSpawner.waveCount;
        currentEnemies = Enemy.numberOfEnemiesAlive;
        gameObject.SetActive(true);
    }
}
