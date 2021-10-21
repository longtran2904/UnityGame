using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI healthText;
    public IntReference playerHealth;

    void Start()
    {
        slider.maxValue = playerHealth.value;
        slider.value = playerHealth.value;
        healthText.text = playerHealth.value.ToString();
    }

    void Update()
    {
        slider.value = playerHealth.value;
        healthText.text = playerHealth.value.ToString();
        if (playerHealth.value < 0)
        {
            healthText.text = "0";
        }
    }
}
