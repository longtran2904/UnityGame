using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour, IPooledObject
{
    public float lifeTime;
    public float increaseAmount;
    public float decreaseAmount;
    public float moveSpeed;
    public Color defaultColor;
    public Color criticalColor;

    private float timer;
    private TextMeshPro text;
    private int sortingOrder;
    Vector3 moveVector;

    void Start()
    {
        text = GetComponent<TextMeshPro>();
    }

    public void OnObjectSpawn()
    {
        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += moveVector * Time.deltaTime;

        if (timer > lifeTime / 2)
        {
            transform.localScale += new Vector3(increaseAmount, increaseAmount) * Time.deltaTime;
        }
        else
        {
            transform.localScale -= new Vector3(decreaseAmount, decreaseAmount) * Time.deltaTime;
        }

        timer -= Time.deltaTime;
        if (timer < 0)
        {
            float disappearSpeed = 3f;
            text.alpha -= disappearSpeed * Time.deltaTime;
            if (text.alpha < 0)
            {
                gameObject.SetActive(false);
            }
        }
    }

    public static DamagePopup Create(Vector3 pos, int damageAmount, bool isCrittical)
    {
        DamagePopup damagePopup = ObjectPooler.instance.SpawnFromPool<DamagePopup>("DamagePopup", pos, Quaternion.identity);
        damagePopup.Setup(damageAmount, isCrittical);
        return damagePopup;
    }

    void Setup(int damageAmount, bool isCritical)
    {
        text.SetText(damageAmount.ToString());
        if (isCritical)
        {
            text.color = criticalColor;
            text.fontSize = 8;
        }
        sortingOrder++;
        text.sortingOrder = sortingOrder;
        moveVector = new Vector3(moveSpeed, moveSpeed);
    }

    void Reset()
    {
        if (text == null)
        {
            text = GetComponent<TextMeshPro>();
        }
        text.color = defaultColor;
        text.alpha = 1;
        timer = lifeTime;
        transform.localScale = Vector3.one;
    }
}
