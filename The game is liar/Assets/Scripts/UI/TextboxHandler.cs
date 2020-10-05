using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextboxHandler : MonoBehaviour
{
    WeaponManager weaponManager;
    GameObject closestObj;
    GameObject lastObj;

    private void Start()
    {
        weaponManager = GetComponent<WeaponManager>();
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("HasTextbox"));
    }

    private void Update()
    {
        Vector2 size = new Vector2(5, 5);
        closestObj = GetClosestObject(Physics2D.OverlapBoxAll(transform.position, size, 0, LayerMask.GetMask("HasTextbox")));
        ExtDebug.DrawBox(transform.position, size / 2, Quaternion.identity, Color.green);
        if (lastObj != closestObj)
        {
            if (lastObj && lastObj.CompareTag("NPC"))
            {
                lastObj.GetComponent<NPC>().ResetUI(); // Set the textbox.active of npc to false. We need this when the closest obj change or when it null.
            }
        }
        if (closestObj)
        {
            if (closestObj.CompareTag("Weapon") || closestObj.CompareTag("SellWeapon"))
            {
                weaponManager.UpdateWeapon(closestObj.GetComponent<Weapon>());
            }
            else if (closestObj.CompareTag("NPC"))
            {
                closestObj.GetComponent<NPC>().DisplayUI();
            }
            lastObj = closestObj;
        }
    }

    GameObject GetClosestObject(Collider2D[] colliders)
    {
        if (colliders.Length == 0)
        {
            return null;
        }
        int closest = 0;
        for (int i = 0; i < colliders.Length; i++)
        {
            bool closer = MathUtils.sqrDistance(transform.position, colliders[i].transform.position) < MathUtils.sqrDistance(transform.position, colliders[closest].transform.position);
            if (closer)
            {
                closest = i;
            }
        }
        return colliders[closest].gameObject;
    }
}
