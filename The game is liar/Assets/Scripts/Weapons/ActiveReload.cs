using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ActiveReload : MonoBehaviour
{
    private Image slider;
    private GameObject reloadBar;
    private RectTransform grey, white;

    public AnimationCurve curve;
    public float perfectSize = 20;
    public float activeSize = 60;

    public bool isReloading { get; private set; }
    private Weapon weapon;
    private ShootAndRotateGun shootAndRotate;
    private WeaponSwitching weaponSwitch;

    private void Start()
    {
        weapon = GetComponent<Weapon>();

        // NOTE: Fix reload bar and slider to be more elegant
        reloadBar = transform.parent.parent.Find("Canvas").Find("ReloadBar").gameObject;
        slider = reloadBar.transform.Find("Slider").GetComponent<Image>();

        grey = reloadBar.transform.GetChild(0).GetComponent<RectTransform>();
        white = reloadBar.transform.GetChild(1).GetComponent<RectTransform>();
        grey.sizeDelta = new Vector2(activeSize, grey.sizeDelta.y);
        white.sizeDelta = new Vector2(perfectSize, white.sizeDelta.y);

        shootAndRotate = GetComponentInParent<ShootAndRotateGun>();
        weaponSwitch = GetComponentInParent<WeaponSwitching>();
    }

    private void Update()
    {
        if (!isReloading)
        {
            if (weapon.currentAmmo <= 0 && Input.GetMouseButton(0))
            {
                BeginReload();
            }
            else if (Input.GetKeyDown(KeyCode.R) && weapon.currentAmmo < weapon.stat.ammo)
            {
                BeginReload();
            } 
        }
    }

    private void LateUpdate()
    {
        // NOTE: the reload bar is a child object of the player because i want to move it along the player but don't want to change its rotation
        if (reloadBar) reloadBar.transform.rotation = Quaternion.identity;
    }

    public void BeginReload()
    {
        isReloading = true;
        shootAndRotate.enabled = false;

        slider.color = Color.white;
        reloadBar.SetActive(true);
        StartCoroutine(Reloading());
    }

    private IEnumerator Reloading()
    {
        float reloadRange = reloadBar.GetComponent<RectTransform>().sizeDelta.x;
        float activePos = Random.Range(reloadRange * .15f, reloadRange - activeSize - reloadRange * .25f);
        float perfectPos = Random.Range(activePos, activePos + activeSize); // The perfect bar has to be inside the active bar
        white.anchoredPosition = new Vector2(perfectPos, 0);
        grey.anchoredPosition = new Vector2(activePos, 0);
        yield return new WaitForSeconds(.001f);
        for (float t = 0; t < 1; t += Time.deltaTime / weapon.stat.standardReload)
        {
            float value = Mathf.Lerp(0, reloadRange, curve.Evaluate(t));
            slider.rectTransform.anchoredPosition = new Vector2(value, 0);
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (MathUtils.InRange(perfectPos, perfectPos + perfectSize, value))
                {
                    ReloadHandler(value, ReloadType.Perfect);
                    yield break;
                }
                else if (MathUtils.InRange(activePos, activePos + activeSize, value))
                {
                    ReloadHandler(value, ReloadType.Fast);
                    yield break;
                }
                else
                {
                    ReloadHandler(value, ReloadType.Failed);
                    yield break;
                }
            }
            yield return null;
        }
        StartCoroutine(FinishReload(0.0f, false));
    }

    enum ReloadType { Perfect, Fast, Failed }
    void ReloadHandler(float value, ReloadType type)
    {
        float t = Mathf.InverseLerp(0, reloadBar.GetComponent<RectTransform>().sizeDelta.x, value);
        slider.color = type == ReloadType.Perfect ? Color.green : type == ReloadType.Fast ? Color.blue : Color.red;
        float remaining = (type == ReloadType.Perfect ? weapon.stat.perfectReload : ((type == ReloadType.Fast) ? weapon.stat.activeReload : weapon.stat.failedReload)) - 
            (t * weapon.stat.standardReload);
        StartCoroutine(FinishReload(remaining, true));
    }

    private IEnumerator FinishReload(float duration, bool perfect)
    {
        yield return new WaitForSeconds(duration);
        slider.rectTransform.anchoredPosition = new Vector2(0, 0);
        reloadBar.SetActive(false);
        weapon.currentAmmo = weapon.stat.ammo;
        isReloading = false;
        weaponSwitch.enabled = true;
    }
}
