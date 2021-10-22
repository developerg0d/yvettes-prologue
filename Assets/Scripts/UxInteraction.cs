using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class UxInteraction : MonoBehaviour
{
    public Image bossHpBar;

    public GameObject golemHand;

    public Image indicator;

    [SerializeField]
    protected Camera mainCamera;

    public float indicatorTimeout = 0.5F;

    public void updateBossHpBar(int currentHp)
    {
        float convertedHp = currentHp * 0.001f;
        bossHpBar.fillAmount = convertedHp;
    }

    public void updateGolemFistIndicatorPosition(Vector3 golemHandPosition)
    {
        indicator.gameObject.SetActive(true);
     //   Debug.Log(mainCamera);
   //    indicator.transform.position = new Vector3((mainCamera.WorldToScreenPoint(golemHandPosition).x), indicator.transform.position.y, 0);
        StartCoroutine("disableIndicator");
    }
    IEnumerator disableIndicator()
    {
        yield return new WaitForSeconds(indicatorTimeout);
        indicator.gameObject.SetActive(false);
    }
}
