using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EqualDistribution : MonoBehaviour
{
    public Vector2Int size;
    public Vector2 distanceBetween;

    public bool overrideScale;
    [ShowWhen("overrideScale")] public Vector3 newScale;

    public enum PivotDirection { TopLeft, BottomLeft, BottomRight, TopRight };
    public PivotDirection pivot;

    public GameObject spawnObject;

    void OnValidate()
    {
        ResetObject();
        SpawnObjects();
    }

    void ResetObject()
    {
        foreach (Transform child in transform)
        {
            StartCoroutine(GameUtils.DestroyInEditor(child.gameObject));
        }
    }

    void SpawnObjects()
    {
        Vector2 spawnDir = new Vector2(1, -1);
        switch (pivot)
        {
            case PivotDirection.BottomLeft:
                spawnDir = new Vector2(1, 1);
                break;
            case PivotDirection.BottomRight:
                spawnDir = new Vector2(-1, 1);
                break;
            case PivotDirection.TopRight:
                spawnDir = new Vector2(-1, -1);
                break;
        }

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                GameObject spawn = Instantiate(spawnObject, transform.position + new Vector3(distanceBetween.x * x * spawnDir.x, distanceBetween.y * y * spawnDir.y), Quaternion.identity, transform);
                if (overrideScale) spawn.transform.localScale = newScale;
            }
        }
    }
}
