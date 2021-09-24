using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UxInteraction : MonoBehaviour
{
    public Image bossHpBar;

    public void updateBossHpBar(int currentHp)
    {
        float convertedHp = currentHp * 0.001f;
        bossHpBar.fillAmount = convertedHp;
    }
}
