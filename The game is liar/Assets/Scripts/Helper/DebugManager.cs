using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    bool isDebug;

    // Start is called before the first frame update
    void Start()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            Time.timeScale = 0;
        }
        if (Input.GetKeyDown(KeyCode.Less))
        {
            Time.timeScale -= .25f;
        }
        if (Input.GetKeyDown(KeyCode.Greater))
        {
            Time.timeScale += .25f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
