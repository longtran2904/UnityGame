using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossManager : MonoBehaviour
{
    public Transform rightCheck, leftCheck;
    public Transform[] spikeSpawns;
    public GameObject spike;
    private Animator anim;
    private JumpBehaviour jumpBehaviour;
    private int spikeFallCounts;
    public Text text;

    void Start()
    {
        anim = GetComponent<Animator>();
        jumpBehaviour = anim.GetBehaviour<JumpBehaviour>();
    }

    void Update()
    {
        RaycastHit2D hitInfoRight;
        RaycastHit2D hitInfoLeft;

        hitInfoRight = Physics2D.Raycast(rightCheck.position, Vector2.up, 50f);
        hitInfoLeft = Physics2D.Raycast(leftCheck.position, Vector2.up, 50f);

        if (hitInfoLeft && hitInfoLeft.transform.tag == "Boss")
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else if (hitInfoRight && hitInfoRight.transform.tag == "Boss")
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        
        if (jumpBehaviour && jumpBehaviour.spikeFalling)
        {
            Debug.Log("b");
            StartCoroutine(SpawnSpikes());
            spikeFallCounts += 1;
            jumpBehaviour.spikeFalling = false;
        }
    }

    IEnumerator SpawnSpikes()
    {
        foreach (Transform spikeSpawn in ChooseSet(10))
        {
            Instantiate(spike, spikeSpawn.position, spike.transform.rotation);
            yield return new WaitForSeconds(0.25f);
        }
        yield return new WaitForSeconds(2.2f);
        if (spikeFallCounts == 3)
        {
            Time.timeScale = 0;
            text.gameObject.SetActive(true);
        }
    }

    Transform[] ChooseSet(int numRequired)
    {
        Transform[] result = new Transform[numRequired];

        int numToChoose = numRequired;

        for (int numLeft = spikeSpawns.Length; numLeft > 0; numLeft--)
        {

            float prob = (float)numToChoose / (float)numLeft;

            if (Random.value <= prob)
            {
                numToChoose--;
                result[numToChoose] = spikeSpawns[numLeft - 1];

                if (numToChoose == 0)
                {
                    break;
                }
            }
        }
        return result;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag.Equals("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

}
