using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spine : MonoBehaviour
{
    public bool flying = false;
    public float timer;
    public int speed;

    // Start is called before the first frame update
    void Update()
    {
        if (flying == true)
        {
            transform.position += transform.up * speed * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag.Equals("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else if (collision.tag.Equals("Ground"))
        {
            InternalDebug.Log("c");
            speed = 0;
            Destroy(gameObject, 2);
        }
        else if (collision.tag.Equals("Boss"))
        {
            Destroy(gameObject);
        }
    }
}
