using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    public Image healthBarImage; 

    public int maxHealthPoints = 10;

    public void UpdateHealthBar(int value)
    {
        healthBarImage.fillAmount = Mathf.Clamp(1.0f * value / maxHealthPoints, 0, 1f);
    }
}
