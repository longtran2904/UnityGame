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
}
