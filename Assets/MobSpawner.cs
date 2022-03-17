using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneTemplate;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    public GameObject enemy;

    public float spawnDelay;
    public Transform spawnPosition;

    private bool isSpawning;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") && !isSpawning)
        {
            isSpawning = true;
            StartCoroutine(nameof(spawning));
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            isSpawning = false;
            StopCoroutine(nameof(spawning));
        }
    }

    IEnumerator spawning()
    {
        while (isSpawning)
        {
            Instantiate(enemy, spawnPosition.position, transform.rotation);
            yield return new WaitForSeconds(spawnDelay);
        }
    }
}