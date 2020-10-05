using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ProceduralLevelGenerator.Unity.Generators.DungeonGenerator;
using UnityEngine.Tilemaps;

public class GeneratorHandler : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        // Find the generator (MAKE SURE THAT THE GAME OBJECT NAME IS CORRECT)
        var generator = GameObject.Find("Dungeon Generator").GetComponent<DungeonGenerator>();

        var levelGenerated = false;

        // Loop until a level is successfully generated
        while (!levelGenerated)
        {
            try
            {
                // Try to generate the level
                generator.Generate();

                // If we get here, that means that there was no timeout
                Debug.Log("Level generated");
                levelGenerated = true;
                //GameObject.Find("RoomManager").SetActive(true);
            }
            catch (InvalidOperationException)
            {
                // If we get here, there was a timeout
                Debug.Log("Timeout encountered");
            }
        }
    }
}
