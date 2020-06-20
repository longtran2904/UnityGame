using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Teleporter : MonoBehaviour
{
    public string destination;

    Player player;

    bool hasPlay = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        player.tpDelegate += Teleport;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (hasPlay)
        {
            return;
        }
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            player.tpDelegate += Teleport;
        }
        player.PlayTeleportAnimation();
        hasPlay = true;
    }

    void Teleport()
    {
        player.tpDelegate -= Teleport;
        GameManager.instance.LoadGame((int)SceneIndexes.BOSS, true);
    }
}
