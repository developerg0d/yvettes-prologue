using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class UxInteraction : MonoBehaviour
{
    public Image bossHpBar;

    public GameObject golemHand;

    public Image indicator;

    [SerializeField] protected Camera mainCamera;


    public Sprite[] arrowSprites;

    public enum ArrowAngles
    {
        Deg0,
        Deg45,
        Deg90,
        Deg135,
        Deg180,
        Deg225,
        Deg270,
        Deg315,
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.LeftShift))
        {
            SceneManager.LoadScene(0);
        }
    }

    public void updateBossHpBar(int currentHp)
    {
        float convertedHp = currentHp * 0.001f;
        bossHpBar.fillAmount = convertedHp;
    }

    public void updateGolemFistIndicatorPosition(Vector3 golemHandPosition, Vector3 playerPosition,
        float disableTimeLength = 0.5F)
    {
        indicator.gameObject.SetActive(true);
        float angle = AngleInDeg(golemHandPosition, playerPosition);
        Debug.Log(angle);
        determineArrowSprite(angle);
        StartCoroutine("disableIndicator", disableTimeLength);
    }

    void determineArrowSprite(float angle)
    {
        float roundedAngle = Mathf.Round(angle * -1);
        int determinedAngle = 0;

        if (determineAngle(roundedAngle, 0, 44))
        {
            determinedAngle = (int) ArrowAngles.Deg0;
        }
        else if (determineAngle(roundedAngle, 45, 89))
        {
            determinedAngle = (int) ArrowAngles.Deg45;
        }
        else if (determineAngle(roundedAngle, 90, 134))
        {
            determinedAngle = (int) ArrowAngles.Deg90;
        }
        else if (determineAngle(roundedAngle, 135, 179))
        {
            determinedAngle = (int) ArrowAngles.Deg135;
        }
        else if (determineAngle(roundedAngle, 180, 224))
        {
            determinedAngle = (int) ArrowAngles.Deg180;
        }
        else if (determineAngle(roundedAngle, 225, 269))
        {
            determinedAngle = (int) ArrowAngles.Deg225;
        }
        else if (determineAngle(roundedAngle, 270, 314))
        {
            determinedAngle = (int) ArrowAngles.Deg270;
        }
        else if (determineAngle(roundedAngle, 315, 359))
        {
            determinedAngle = (int) ArrowAngles.Deg315;
        }

        indicator.sprite = arrowSprites[determinedAngle];
    }

    public bool determineAngle(float currentAngle, float minAngle, float maxAngle)
    {
        return currentAngle >= minAngle && currentAngle <= maxAngle;
    }

    private float AngleInRad(Vector3 vec1, Vector3 vec2)
    {
        return Mathf.Atan2(vec2.y - vec1.y, vec2.x - vec1.x);
    }

    private float AngleInDeg(Vector3 vec1, Vector3 vec2)
    {
        return AngleInRad(vec1, vec2) * 180 / Mathf.PI;
    }

    public void disableUiOnDeath()
    {
        StopCoroutine(nameof(disableIndicator));
        indicator.gameObject.SetActive(false);
    }

    IEnumerator disableIndicator(float disableIndicatorLength)
    {
        yield return new WaitForSeconds(disableIndicatorLength);
        indicator.gameObject.SetActive(false);
    }
}