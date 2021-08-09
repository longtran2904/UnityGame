using UnityEngine;
using System;
using Edgar.Unity;

public class GeneratorHandler : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        // Find the generator (MAKE SURE THAT THE GAME OBJECT NAME IS CORRECT)
        var generator = GameObject.Find("Dungeon Generator").GetComponentInParent<DungeonGenerator>();

        var levelGenerated = false;

        // Loop until a level is successfully generated
        while (!levelGenerated)
        {
            try
            {
                // Try to generate the level
                generator.Generate();

                // If we get here, that means that there was no timeout
                InternalDebug.Log("Level generated");
                levelGenerated = true;
                //GameObject.Find("RoomManager").SetActive(true);
            }
            catch (InvalidOperationException)
            {
                // If we get here, there was a timeout
                InternalDebug.Log("Timeout encountered");
            }
        }
    }
}
