using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Enemies
{
    public Slime splitSlime;

    public override void Die()
    {
        if (splitSlime)
        {
            Split();
        }
        else
        {
            DropMoney(moneyDropRange.GetRandom());
        }
        GameObject explosion = explosionParitcle;
        Instantiate(explosion, transform.position, Quaternion.identity);
        numberOfEnemiesAlive--;
        Destroy(gameObject);
    }

    void Split()
    {
        Vector3 offset = new Vector3(.5f, 0, 0);
        float randomTimeOffset = Random.Range(0f, .8f);
        Instantiate(splitSlime, transform.position + offset, Quaternion.identity).hasSpawnVFX = false;
        Slime slime = Instantiate(splitSlime, transform.position + offset, Quaternion.identity);
        slime.hasSpawnVFX = false;
        slime.StartCoroutine(Delay());
        
        IEnumerator Delay()
        {
            slime.GetComponent<SlimeMovement>().enabled = false;
            yield return new WaitForSeconds(randomTimeOffset);
            slime.GetComponent<SlimeMovement>().enabled = true;
        }
    }
}
