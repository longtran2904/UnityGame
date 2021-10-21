using UnityEngine;

public class AudioObject : MonoBehaviour
{
    public AudioManager manager;

    private void Awake()
    {
        if (!AudioManager.instance)
        {
            DontDestroyOnLoad(gameObject);
            manager.Init();
        }
    }
}
