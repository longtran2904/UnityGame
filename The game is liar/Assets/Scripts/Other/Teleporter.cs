using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public string destination;

    Player player;

    bool hasPlay = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (hasPlay)
        {
            return;
        }
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        player.teleportEvent += Teleport;
        player.PlayTeleportAnimation();
        hasPlay = true;
    }

    void Teleport()
    {
        player.teleportEvent -= Teleport;
        GameManager.instance.LoadGame((int)SceneIndexes.BOSS, true);
    }
}
