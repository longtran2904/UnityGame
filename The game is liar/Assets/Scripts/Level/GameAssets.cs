using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public GameObject[] stateParticles;

    public static GameAssets instance;

    void Awake()
    {
        instance = this;
    }
}
