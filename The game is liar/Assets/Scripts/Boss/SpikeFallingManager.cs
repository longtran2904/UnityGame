using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeFallingManager : MonoBehaviour
{
    public GameObject[] flyingSpikes;
    private Spine[] spikes;
    public float timeBtwSpikes;
    public int speed;

    // Start is called before the first frame update
    void Start()
    {
        spikes = new Spine[flyingSpikes.Length];
        int i = 0;
        foreach (GameObject flyingSpike in flyingSpikes)
        {
            spikes[i] = flyingSpike.GetComponentInChildren<Spine>();
            InternalDebug.Log(spikes[i]);
            i++;
        }
        StartCoroutine(FallingSpikes());
    }

    IEnumerator FallingSpikes()
    {
        foreach (Spine spike in spikes)
        {
            if (speed != 0)
            {
                spike.speed = speed;
            }
            spike.flying = true;
            yield return new WaitForSeconds(timeBtwSpikes);
        }
    }
}
