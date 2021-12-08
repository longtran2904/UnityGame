using UnityEngine;
using System;
using Edgar.Unity;

public class GeneratorHandler : MonoBehaviour
{
    public int maxTryNumber = 10;

    // Start is called before the first frame update
    private void Start()
    {
        var generator = GameObject.Find("Dungeon Generator").GetComponent<DungeonGenerator>();
        bool levelGenerated = false;
        int count = maxTryNumber;

        while (!levelGenerated)
        {
            try
            {
                if (count == 0)
                {
                    Debug.LogError("Level couldn't be generated!");
                    break;
                }
                generator.Generate();
                levelGenerated = true;
            }
            catch (InvalidOperationException)
            {
                // If we get here, there was a timeout
                Debug.LogError("Timeout encountered");
                count--;
            }
        }
    }
}
