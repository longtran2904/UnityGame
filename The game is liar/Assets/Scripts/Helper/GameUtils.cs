using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameUtils
{
    public static IEnumerator Deactive(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);

        obj.SetActive(false);
    }

    public static void DisableAllChildObjects(Transform objTransform)
    {
        foreach (Transform child in objTransform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public static void EnableAllChildObjects(Transform objTransform)
    {
        foreach (Transform child in objTransform)
        {
            child.gameObject.SetActive(true);
        }
    }

    public static void DisableAllMonoBehaviours(GameObject obj)
    {
        foreach (var behaviour in obj.GetComponents<MonoBehaviour>())
        {
            behaviour.enabled = false;
        }
    }

    public static void EnableAllMonoBehaviours(GameObject obj)
    {
        foreach (var behaviour in obj.GetComponents<MonoBehaviour>())
        {
            behaviour.enabled = true;
        }
    }

    public static void DisableAllBehaviours(GameObject obj)
    {
        foreach (var behaviour in obj.GetComponents<Behaviour>())
        {
            behaviour.enabled = false;
        }
    }

    public static void EnableAllBehaviours(GameObject obj)
    {
        foreach (var behaviour in obj.GetComponents<Behaviour>())
        {
            behaviour.enabled = true;
        }
    }
}
