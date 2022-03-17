using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialIndicator : MonoBehaviour
{
    public GameObject uxIndicator;
    public Transform indicatorLocation;
    public Camera mainCamera;

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            uxIndicator.SetActive(true);
            uxIndicator.transform.position = mainCamera.WorldToScreenPoint(indicatorLocation.position);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            uxIndicator.SetActive(false);
        }
    }
}