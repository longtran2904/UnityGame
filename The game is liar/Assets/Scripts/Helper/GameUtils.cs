using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameUtils
{
    public static void Clear(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            Object.Destroy(child.gameObject);
        }
    }

    public static IEnumerator Deactive(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }

    // Use this to destroy in the OnValidate or in the editor
    public static IEnumerator DestroyInEditor(GameObject go)
    {
        yield return new WaitForEndOfFrame();
        Object.DestroyImmediate(go);
    }
}
