using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveReload : MonoBehaviour
{
    public Image slider;
    public GameObject reloadBar;
    public AnimationCurve curve;
    public Vector2 perfectRange = new Vector2(150, 170);
    public Vector2 activeRange = new Vector2(170, 230);
    private Coroutine reload;
    private Weapon weapon;

    private void Start()
    {
        weapon = GetComponent<Weapon>();
        weapon.reloadingDelegate += BeginReload;
    }

    public void BeginReload()
    {
        if (reloadBar == null)
        {
            reloadBar = GameObject.Find("Canvas").transform.Find("ReloadBar").gameObject;
            slider = reloadBar.transform.Find("Slider").GetComponent<Image>();
        }
        slider.color = Color.white;
        reloadBar.SetActive(true);
        reload = StartCoroutine(Reloading());
    }

    private IEnumerator Reloading()
    {
        yield return new WaitForSeconds(.001f);
        for (float t = 0; t < 1; t += Time.deltaTime / weapon.standardReload)
        {
            float value = Mathf.Lerp(0, reloadBar.GetComponent<RectTransform>().sizeDelta.x, curve.Evaluate(t));
            slider.rectTransform.anchoredPosition = new Vector2(value, 0);
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (MathUtils.InRange(perfectRange.x, perfectRange.y, value))
                {
                    PerfectReload(value);
                    yield break;
                }
                else if (MathUtils.InRange(activeRange.x, activeRange.y, value))
                {
                    FastReload(value);
                    yield break;
                }
                else
                {
                    FailedReload(value);
                    yield break;
                }
            }
            yield return null;
        }
        StartCoroutine(FinishReload(0.0f, false));
    }

    private IEnumerator FinishReload(float duration, bool perfect)
    {
        yield return new WaitForSeconds(duration);
        slider.rectTransform.anchoredPosition = new Vector2(0, 0);
        /*if (perfect)
            PerfectReload();
        else
            StandardReload();*/
        reloadBar.SetActive(false);
        weapon.SetAmmo();
        weapon.ammoText.SetText("{0}/{1}", (float)weapon.maxAmmo, (float)weapon.maxAmmo);
    }

    void PerfectReload(float value)
    {
        slider.color = Color.green;
        float t = Mathf.InverseLerp(0, reloadBar.GetComponent<RectTransform>().sizeDelta.x, value);
        float remaining = weapon.perfectReload - (t * weapon.standardReload);
        StartCoroutine(FinishReload(remaining, true));
    }

    private void FastReload(float value)
    {
        slider.color = Color.blue;
        float t = Mathf.InverseLerp(0, reloadBar.GetComponent<RectTransform>().sizeDelta.x, value);
        float remaining = weapon.activeReload - (t * weapon.standardReload);
        StartCoroutine(FinishReload(remaining, false));
    }

    private void FailedReload(float value)
    {
        slider.color = Color.red;
        float t = Mathf.InverseLerp(0, reloadBar.GetComponent<RectTransform>().sizeDelta.x, value);
        float remaining = weapon.failedReload - (t * weapon.standardReload);
        StartCoroutine(FinishReload(remaining, false));
    }
}
