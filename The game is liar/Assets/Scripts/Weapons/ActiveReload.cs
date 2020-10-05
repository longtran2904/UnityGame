using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveReload : MonoBehaviour
{
    public Image slider;
    public GameObject reloadBar;
    public AnimationCurve curve;
    public float perfectRange = 20;
    public float activeRange = 60;
    private Weapon weapon;

    private RectTransform grey, white;

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
            grey = reloadBar.transform.GetChild(0).GetComponent<RectTransform>();
            white = reloadBar.transform.GetChild(1).GetComponent<RectTransform>();
            grey.sizeDelta = new Vector2(activeRange, grey.sizeDelta.y);
            white.sizeDelta = new Vector2(perfectRange, white.sizeDelta.y);
        }
        slider.color = Color.white;
        reloadBar.SetActive(true);
        StartCoroutine(Reloading());
    }

    private IEnumerator Reloading()
    {
        float reloadRange = reloadBar.GetComponent<RectTransform>().sizeDelta.x;
        float activePos = Random.Range(reloadRange * .15f, reloadRange - activeRange - reloadRange * .75f);
        float perfectPos = Random.Range(activePos, activePos + activeRange); // The perfect bar has to be inside the active bar
        white.anchoredPosition = new Vector2(perfectPos, 0);
        grey.anchoredPosition = new Vector2(activePos, 0);
        yield return new WaitForSeconds(.001f);
        for (float t = 0; t < 1; t += Time.deltaTime / weapon.stat.standardReload)
        {
            float value = Mathf.Lerp(0, reloadRange, curve.Evaluate(t));
            slider.rectTransform.anchoredPosition = new Vector2(value, 0);
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (MathUtils.InRange(perfectPos, perfectPos + perfectRange, value))
                {
                    PerfectReload(value);
                    yield break;
                }
                else if (MathUtils.InRange(activePos, activePos + activeRange, value))
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
        reloadBar.SetActive(false);
        weapon.SetAmmo();
        weapon.ammoText.SetText("{0}/{1}", (float)weapon.stat.ammo, (float)weapon.stat.ammo);
    }

    void PerfectReload(float value)
    {
        slider.color = Color.green;
        float t = Mathf.InverseLerp(0, reloadBar.GetComponent<RectTransform>().sizeDelta.x, value);
        float remaining = weapon.stat.perfectReload - (t * weapon.stat.standardReload);
        StartCoroutine(FinishReload(remaining, true));
    }

    private void FastReload(float value)
    {
        slider.color = Color.blue;
        float t = Mathf.InverseLerp(0, reloadBar.GetComponent<RectTransform>().sizeDelta.x, value);
        float remaining = weapon.stat.activeReload - (t * weapon.stat.standardReload);
        StartCoroutine(FinishReload(remaining, false));
    }

    private void FailedReload(float value)
    {
        slider.color = Color.red;
        float t = Mathf.InverseLerp(0, reloadBar.GetComponent<RectTransform>().sizeDelta.x, value);
        float remaining = weapon.stat.failedReload - (t * weapon.stat.standardReload);
        StartCoroutine(FinishReload(remaining, false));
    }
}
