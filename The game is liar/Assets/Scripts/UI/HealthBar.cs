using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public Slider slider;

    public TextMeshProUGUI healthText;

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
        healthText.text = health.ToString();
    }

    public void SetHealth(int health)
    {
        slider.value = health;
        healthText.text = health.ToString();
        if (health < 0)
        {
            healthText.text = "0";
        }
    }

    public void SetHealth(int health, bool canNegative)
    {
        slider.value = health;
        healthText.text = health.ToString();
        if (canNegative == false && health < 0)
        {
            healthText.text = "0";
        }
    }
}
