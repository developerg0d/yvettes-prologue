using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Checkpoint : MonoBehaviour
{
    public bool activated;

    public Sprite activatedSprite;
    public GameObject[] checkpoints;
    public int currentIndex;
    
    void Start()
    {
        checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
        for (var i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] == gameObject)
            {
                currentIndex = i;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player") && !activated)
        {
            activated = true;
            GetComponent<SpriteRenderer>().sprite = activatedSprite;
        }
    }
}