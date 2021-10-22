using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEngine.Serialization;

public class BossStateManager : MonoBehaviour
{
    [Header("Boss State Manager")]
    [Space(10)]

    [Tooltip("This script allows you to set an array of Boss Stage Events.This will allow you to customize the Boss Event Lifecycle for each boss how you please.")]
    public UnityEvent[] bossStageEvents;

    public int currentStage;

    void Start()
    {
        UpdateBossStage();
    }

    public void NextBossStage()
    {
        currentStage += 1;
        bossStageEvents[currentStage].Invoke();
    }
    private void UpdateBossStage()
    {
        bossStageEvents[currentStage].Invoke();
    }

}
