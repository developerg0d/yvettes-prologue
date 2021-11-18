using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public bool fxOn;
    public bool musicOn;

    private AudioSource audioSource;

    public AudioClip beenHitSound;
    public AudioClip crashSound;

    public Text fxToggleText;
    public Text musicToggleText;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void toggleFx()
    {
        if (fxOn)
        {
            fxToggleText.text = "Fx Off";
            fxOn = false;
            return;
        }

        fxToggleText.text = "Fx On";
        fxOn = true;
    }

    public void toggleMusic()
    {
        if (musicOn)
        {
            musicToggleText.text = "Music Off";
            musicOn = false;
            return;
        }

        musicToggleText.text = "Music On";
        musicOn = true;
    }

    public void playCrashSound()
    {
        audioSource.PlayOneShot(crashSound);
    }

    public void playBeenHitSound()
    {
        audioSource.PlayOneShot(beenHitSound);
    }
}